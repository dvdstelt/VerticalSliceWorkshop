(function () {
    angular.module('app.controllers')
        .controller('ordersController',
        ['$log', 'endpoints.config', '$http',
            function ($log, config, $http) {

                var ctrl = this;

                ctrl.isLoading = null;
                ctrl.orders = null;

                ctrl.refreshOrders = function () {
                    ctrl.isLoading = $http.get(config.apiBaseUrl + '/orders/')
                        .then(function (response) {
                            ctrl.orders = response.data;
                        })
                        .catch(function (error) {
                            $log.error('Something went wrong: ', error);
                            ctrl.error = 'Something went wrong. Look at the console log in your browser';
                        });
                };

                ctrl.createNewOrder = function () {

                    var payload = {
                        customerId: Math.floor(Math.random() * 3) + 1, //Valid values: 1,2,3
                        products: Array.from({length: Math.floor(Math.random() * 3) + 1}, () => ({
                            productId: Math.floor(Math.random() * 6) + 1 //Valid values: 1 -> 6
                        }))
                    };

                    return $http.post(config.apiBaseUrl + '/orders/', payload)
                        .then(function (createOrderResponse) {
                            $log.debug('raw order created:', createOrderResponse.data);
                            return createOrderResponse.data;
                        });
                };

                ctrl.refreshOrders();
            }]);
}())