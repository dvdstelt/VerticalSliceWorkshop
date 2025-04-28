# Exercise  1: Vertical Slices

In this exercise, we'll see how vertical slices are working, add additional vertical slices, and a behavior to validate our data.

## Overview

When you open the solution, you see the structure of our solution, which has 4 projects:

1. `Divergent.Api`
2. `Divergent.Data`
3. `Divergent.Frontend`
4. `Shared`

#### What is each project responsible for?

The `Api` in our case is the central part of our application. This is where our vertical slices are located. It could've been separated up more, so we could reuse our vertical slices, features, behavior, etc., in a separate user interface or anything. But to keep it simple, it was kept this way.

The `Data` project was split up because other exercises with a different solution structure use a similar setup. This project contains our model and context to retrieve data from a [SQLite database](https://www.sqlite.org/). SQLite is a single file on disk that contains our database and supports multiple threads working with its data, without the need to install anything. It's a single NuGet package, already included in the project.

The `Frontend` website is an Angular website that retrieves data using our API. This website is (relatively) similar throughout all exercises.

The `Shared` project contains configuration for SQLite and logging.

## Start-up projects

For more info, please see [the instructions for running the exercise solutions](/readme.md#running-the-exercise-solutions).

- `Divergent.Api`
- `Divergent.Frontend`

## Business requirements

The application UI consists of three pages: Dashboard, Orders, and OrderDetails. In this exercise, your goal is to enhance the Orders page to be able to create new orders with a random customer and random products, through the push of a single button. When this happens, we also want a loosely coupled vertical slice that sends an email to the customer with their order. We'll also allow the OrderDetails page to retrieve and display data. We'll also look at behaviors and how we can add a behavior that monitors the performance of our vertical slices.

## Exercise 1.1: Create a new order

#### Step 1

In the project `Divergent.Frontend` have a look at the `ordersView.html` file. It is inside the following folder: `/app/presentation/`.

There is a button with the following text: *Create new order*

This button calls the method `createNewOrder` on our controller. Let's have a look at it.

### Step 2

In the file `ordersController.js` take a look at the `createNewOrder` method. It first creates a payload:

```c#
var payload = {
    customerId: Math.floor(Math.random() * 3) + 1, //Valid values: 1,2,3
    products: Array.from({length: Math.floor(Math.random() * 3) + 1}, () => ({
        productId: Math.floor(Math.random() * 6) + 1 //Valid values: 1 -> 6
    }))
};
```

This creates a random customer identifier from 1 to 3. Those are the customers already in our database.

> [!note]
>
> If you want to have a look at the data already generated, go to the `Divergent.Data` project, inside the `Migrations` folder there's a class called `SeedData` which shows what customers and products are generated. By default, two orders are also already created so we have something to display.

It then adds between 1 and 3 items to our order, each with a product identifier between 1 and 6. Those are the movies added to our product catalog.

It then does an HTTP Post to the backend using this code:

```c#
return $http.post(config.apiBaseUrl + '/orders/', payload)
```

There's additional code to log to the console if the order was created or an error is displayed if it couldn't. Note that you'll have to refresh the screen yourself after creating an order. This is done for a reason that might become clear in later exercises.

### Step 3

Now let's create the vertical slice that creates the order and stores it inside the database.

In the `Divergent.Api` project go to the `/Features/Orders/` folder and create a new file called `CreateOrder.cs`.

Create a class called `CreateOrder` that implements the ASP.NET `Controller` class. Mark the class with the attribute `ApiController`,  so that it understands where to get parameters from when we HTTP Post into this API.

```c#
[ApiController]
public class CreateOrder : Controller
{
}
```

### Step 4

Create a constructor to inject `IMediator` interface into it and store it inside a variable, or use a [primary constructor](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/primary-constructors).

```c#
[ApiController]
public class CreateOrder(IMediator mediator) : Controller
{
}
```

### Step 5

Create a method called `Post` that responds to an HTTP Post on `/api/orders`.

```c#
[HttpPost("/api/orders")]
public async Task<IActionResult> Post()
{ }
```

To be able to accept the payload from our JavaScript controller, we need a new viewmodel.

```c#
public class OrderViewModel
{
    public int CustomerId { get; set; }
    public List<ProductViewModel> Products { get; set; }
}

public class ProductViewModel
{
    public int ProductId { get; set; }
}
```

Have the OrderViewModel as a parameter of our post method.

```c#
[HttpPost("/api/orders")]
public async Task<IActionResult> Post(OrderViewModel order)
{   
    return Ok(orderId);
}
```

### Step 6

Let's ask MediatR to send a request to store the data we just got from the user interface.

We first need to create a request object called `CreateOrderCommand`. We'll put this inside the same file `CreateOrder.cs` so everything stays close together.

```c#
public record CreateOrderCommand(int CustomerId, List<int> Products) : IRequest<int>;
```

### Step 7

Now in our `Post` method we can ask MediatR to send the request

```c#
var orderId = await mediator.Send(new CreateOrderCommand(order.CustomerId, order.Products.Select(p => p.ProductId).ToList()));
```

### Step 8

We'll have to create a class that accepts this request and stores the order in our database.

Add to the same file a new class called `CreateOrderHandler` and have the DivergentDbContext injected via the constructor.

```c#
internal sealed class CreateOrderHandler(DivergentDbContext db) : IRequestHandler<CreateOrderCommand, int>
{
}
```

Have your favorite IDE implement the missing `Handle` method.

### Step 9

Create the new Order that we'll store using LiteDb

```c#
var order = new Order()
{
    CustomerId = request.CustomerId,
    Items = request.Products
};
```

Store it using LiteDb and return the order id. We currently don't use it, but you never know.

```c#
var orderCollection = db.Database.GetCollection<Order>();
order.Id = orderCollection.Insert(order);

return Task.FromResult(order.Id);
```

### Step 10

Run the exercise and verify if creating the order is working.

After pressing the *Create new order* button, hit the *Refresh* button to reload the data and display the new order.

## Exercise 1.2: Send an email

Once an order is created, we want to email our customer. But we want this in a loosely coupled fashion. So, instead of adding this to our current feature, we want to create a separate feature to execute this request. We will use something in MediatR that allows us to publish events. We'll get more in-depth about publishing events in a future module. For now, it is only essential to understand that our feature sends a notification, without knowing who is listening. We will create a separate feature that responds to this notification and sends the email.

### Step 1

In the project `Divergent.Api`, create a folder `Domain` in the root of the project.

Create a class `OrderCreated` and add a constructor that takes a single integer orderId and store it internally.

```c#
public class OrderCreated(int orderId) : DomainEvent
{
    public int OrderId { get; } = orderId;
}
```

### Step 2

Whenever our order is created, we want this notification to be published by MediatR. Open the file `CreateOrder.cs` and go to the class we just added called `CreateOrderHandler`.

First, we need dependency injection to inject the MediatR publisher into our constructor.

```c#
internal sealed class CreateOrderHandler(DivergentDbContext db, IPublisher publisher) : IRequestHandler<CreateOrderCommand, int>
```

At the very end, use this injected publisher, just before returning the order id, to publish the notification.

```c#
await publisher.Publish(new OrderCreated(order.Id), cancellationToken);
```

### Step 3

In the `Features` folder, create a new subfolder called `Email`.

Create a new class in this folder called `OrderCreatedHandler` and have it inherit the following interface: `INotificationHandler<OrderCreated>`.

### Step 4

Since we only have the identifier of our order and nothing else, we need to retrieve all the data. This might seem counterintuitive, because we could provide all the data immediately from where we published the notification. We had everything already there.

But if something in that feature changes or inside the email, we must also change the other class. We'd have to either retrieve it, pass it from somewhere else and then add it to the notification, before we could change the email.

We'll explain why we only want identifiers in notifications (or events) later in this workshop.

Let's retrieve everything we need for the email.

```c#
var orderCollection = db.Database.GetCollection<Order>();
var order = orderCollection.FindById(notification.OrderId);

var customerCollection = db.Database.GetCollection<Customer>();
var customer = customerCollection.FindById(order.CustomerId);

var productCollection = db.Database.GetCollection<Product>();
var products = productCollection.Query().Where(p => order.Items.Contains(p.Id)).ToList();
```

### Step 5

Create the email message:

```c#
var mailMessage = new MailMessage
{
    From = new MailAddress("orders@divergent.com"),
    To = { new MailAddress(customer.Email) },
    Subject = $"Order #{order.Id} Confirmation",
    Body = $"""
        Dear {customer.Name},

        Thank you for your order #{order.Id}.

        Order details:
        {string.Join(Environment.NewLine, products.Select(p => $"- {p.Name}: ${p.Price}"))}

        Total: ${products.Sum(p => p.Price)}

        Best regards,
        Divergent Team
        """
};
```

### Step 6

Now let's send the email over smtp.

```c#
var smtpClient = new SmtpClient("localhost", 25);
await smtpClient.SendMailAsync(mailMessage, cancellationToken);
```

### Step 7

To be able to receive the email, we could use a nice tool that will act as an SMTP server and also can display the email.

[smtp4dev](https://github.com/rnwood/smtp4dev) is such a tool. In the [releases folder](https://github.com/rnwood/smtp4dev/releases) you can find a version that will suit you. You can either use a Docker image or the Windows standalone Desktop app. I always use the latter one to host and receive my emails.

After downloading this or another application, run the exercise to verify if the email is sent (and received).

> [!note]
>
> If sending an email fails, our order is still stored! Think about how you'd solve this.

## Exercise 1.3: Retrieve order details

So far, we've worked with two vertical slices related to retrieving multiple orders and creating an order. Let's now focus on **OrderDetails**.

### Step 1

In the `Divergent.Frontend` project, find the `ordersView.html` and open it.

There's a line with the following code in it:

```html
<strong>Order: {{order.orderId}}</strong>
```

Change it into the following, which uses Angular to create a link to another page displaying all the details:

```
<a ui-sref="orderDetails({orderId: order.orderId})"><strong>Order: {{order.orderId}}</strong></a>
```

### Step 2

All the HTML and JavaScript is already in place to retrieve order details, since you're not in this workshop to learn those. You can find the HTML in `orderDetailsView.html` and the JavaScript in `orderDetailsController.js`. We can have a quick look though.

In `orderDetailsController.js` the following line calls the API we will have to build:

```javascript
$http.get(config.apiBaseUrl + '/order/' + $stateParams.orderId)
```

### Step 3

In the project `Divergent.Api` in the folder `Features`, create a new folder called `OrderDetails`

In this folder, please create a new file `GetOrderDetails.cs,` and in it create a new class for our API controller.

```c#
public class GetOrderDetails : Controller
{
}
```

We'll need to inject `IMediator` again in the constructor.

### Step 4

Create a new method to obtain the order details for route `/api/order/1` or whatever order identifier we provide:

```c#
[HttpGet("/api/order/{orderId}")]
public async Task<IActionResult> Get([FromRoute] int orderId)
{
    return Ok(result);
}
```

Inside this method, we don't want to access our data layer immediately, but use MediatR to send a request.

```c#
var result = await mediator.Send(new GetOrderDetailsQuery(orderId));
```

### Step 5

We'll need the `GetOrderDetailsQuery` as well. Add it to the same file, so everything is close together. This requires a constructor to enter the `OrderId`. In the provided code below, we used a [primary constructor](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/primary-constructors).

```c#
public record GetOrderDetailsQuery(int OrderId) : IRequest<OrderViewModel>;
```

### Step 6

Create a class that will receive the request for order details.

```c#
internal sealed class GetOrderDetailsHandler(DivergentDbContext db) : IRequestHandler<GetOrderDetailsQuery, OrderViewModel>
{
}
```

Implement the missing method as well.

Now load:

1. The order
2. The customer details
3. The product details

```c#
// Load order
var orderCollection = db.Database.GetCollection<Order>();
var order = orderCollection.Query().Where(o => o.Id == request.OrderId).FirstOrDefault();

// Load customer
var customerCollection = db.Database.GetCollection<Customer>();
var customer = customerCollection.Query().Where(c => c.Id == order.CustomerId).FirstOrDefault();

// Load products
var productCollection = db.Database.GetCollection<Product>();
var products = productCollection.Query().Where(p => order.Items.Contains(p.Id)).ToList();
```

### Step 7

Map everything to the view model object and return the result

```c#
var orderViewModel = new OrderViewModel
{
    OrderId = order.Id,
    Customer = customer != null ? new CustomerViewModel { Id = customer.Id, Name = customer.Name } : null,
    Products = products.Select(p => new ProductViewModel { Id = p.Id, Name = p.Name, Price = p.Price }),
    TotalPrice = products.Sum(p => p.Price)
};

return Task.FromResult(orderViewModel);
```

### Step 8

Rerun the exercise and verify if it shows the order details after clicking the link on the orders page.

### 

> [!important]
>
> Notice how we've reused `OrderViewModel` from our `GetOrders` feature. As a result, whenever the ViewModel for that feature changes, our new feature has to change with it. This is starting to look like tight-coupling. Contemplate a bit how you feel about this.
>
> 1. Would you think this is an issue?
> 2. What is a way to solve this?



## Exercise 1.4: Add validation

Let's add validation using [FluentValidation](https://fluentvalidation.net/), for which the NuGet package was already added to the solution.

### Step 1

In the `Divergent.Api`  project under the `Common` folder, create a new folder called `Behaviors`.

### Step 2

Please create a new class called `ValidationBehavior` and have it implement the interface `IPipelineBehavior<TRequest, TResponse>`.

This interface is part of [MediatR](https://github.com/jbogard/MediatR). The end result would look like this:

```c#
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        return await next(cancellationToken);
    }
}
```

Note that we added `return await next(cancellationToken)` to ensure the next step in the pipeline is called. This can either be our feature or another behavior. For example, a behavior to measure how long it takes to execute a request, which we'll do in exercise 1.5.

### Step 3

Let's add verification of errors. First we need to create the context to validate.

```c#
var context = new ValidationContext<TRequest>(request);
```

Now that we have the validation context, let's execute all validations.

```c#
var validationResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
```

Now let's combine all failures from all validators:

```c#
var failures = validationResults
    .SelectMany(r => r.Errors)
    .Where(f => f != null)
    .ToList();
```

Now, ensure that if any failures are found, throw a `ValidationException`. This is a custom exception as part of FluentValidations that can accept the failures as a collection. You'd then be able to list all failures in a list.

### Step 4

Let's create a validator for when orders are created.

In the `CreateOrder.cs` class created earlier:

- Add an additional class called `CreateOrderCommandValidator` 
- Have it inherit from `AbstractValidator<CreateOrderCommand>`

Create a constructor without parameters for this class and add the following rules to the constructor:

```c#
RuleFor(x => x.CustomerId).GreaterThan(0);
RuleFor(x => x.Products).NotEmpty();
RuleForEach(x => x.Products).GreaterThan(0);
RuleFor(x => x.Products).Must(x => x.Distinct().Count() == x.Count);
```

- This ensures that the identifier for none of the products is 0, which is the initial value for an integer.
- It also ensures the `Products` collection is not empty.
- Lastly, it ensures that each product is only added once.

### Step 5

We will now have FluentValidation scan and register all validators in our assembly so that they can be used to validate requests.

In the `Divergent.Api` projects, in the `ConfigureServices` class, add the following line:

```c#
services.AddValidatorsFromAssembly(typeof(ConfigureServices).Assembly, includeInternalTypes: true);
```

We use the type of our class `ConfigureServices`, to get to the assembly, so that `AddValidatorsFromAssembly` can find all our validators.

### Step 6

We will now add the behavior to the MediatR pipeline so that it can find and execute the appropriate validators.

In the `ConfigureServices` class, add the following line to the `services.AddMediatR` call:

```c#
options.AddOpenBehavior(typeof(ValidationBehavior<,>));
```

As a result, the code should look like this:

```c#
services.AddMediatR(options =>
{
    options.RegisterServicesFromAssembly(typeof(ConfigureServices).Assembly);

    options.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
```

### Step 7

We will now add code to our user interface to display any validation errors.

In the project `Divergent.Frontend`, in the file `ordersController.js` , make sure the code for the **creation of the order** is changed into this:

> [!note]
>
> There's code for retrieval and creation in this JavaScript controller. Modify the correct code!

```c#
return $http.post(config.apiBaseUrl + '/orders/', payload)
    .then(function (createOrderResponse) {
        $log.debug('raw order created:', createOrderResponse.data);
        return createOrderResponse.data;
    })
    .catch(function (error) {
        $log.error('Failed to create order:', error);
        ctrl.error = 'Failed to create order: ' + error.data.title;
        throw error;
    });
```

### Step 8

We've added validation, which should be executed whenever an order is created. Since the code should only create valid orders, try to change the validation rules in such a way that it will report validation errors upon creating a new order.

## Exercise 1.5: Measure the performance of our vertical slices

We want to monitor how long each vertical slice takes to execute, and if it takes longer than 500 milliseconds, we want to log this as a warning.

> [!note]
>
> This exercise won't give as detailed information since we've already implemented a behavior.

### Step 1

In the `Behaviors` folder, create a new class called `PerformanceBehavior` and have it implement `IPipelineBehavior<TRequest, TResponse>`.

### Step 2

Set up a `Stopwatch` to measure the performance. The following line of code calls the next step in the pipeline. Surround it by starting and stopping the created `Stopwatch` object.

```c#
var response = await next(cancellationToken);
```

Make sure to return the response at the end of the behavior.

### Step 3

Calculate how long it took to execute the pipeline's next step(s). Log the execution time and the name of the request using `typeof(TRequest).Name`, but make it a warning when the execution time is over 500ms.

### Step 4

Now register the new behavior in `ConfigureServices.cs` and add the following line to the `services.AddMediatR` code:

```c#
options.AddOpenBehavior(typeof(PerformanceBehavior<,>));
```

### Step 5

Now execute the code and verify log entries.



## Conclusion

We see that we've created multiple vertical slices, adding features to our application. We've combined some of them in the same folder.

As a result, every single feature (or use case) is represented in code that belongs together, but isn't depended on any other code in our solution.

This is high cohesion, low coupling. Or is it?