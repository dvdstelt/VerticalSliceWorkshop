(function () {
    angular.module('app.controllers')
        .controller('orderDetailsController',
            ['$log', 'endpoints.config', '$http', '$stateParams',
                function ($log, config, $http, $stateParams) {

                    var ctrl = this;

                    ctrl.isLoading = true;
                    ctrl.order = null;
                    ctrl.error = null;

                    ctrl.loadOrderDetails = function () {
                        ctrl.isLoading = true;
                        $http.get(config.apiBaseUrl + '/order/' + $stateParams.orderId)
                            .then(function (response) {
                                ctrl.order = response.data;
                                ctrl.isLoading = false;
                            })
                            .catch(function (error) {
                                $log.error('Failed to load order details:', error);
                                ctrl.error = 'Failed to load order details. Please try again.';
                                ctrl.isLoading = false;
                            });
                    };

                    // Load the order details when controller initializes
                    ctrl.loadOrderDetails();
                }]);
}())