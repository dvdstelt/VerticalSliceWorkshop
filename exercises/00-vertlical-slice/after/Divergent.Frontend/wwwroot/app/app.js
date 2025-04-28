/* global angular */
(function () {

    'use strict';

    angular.module('app.controllers', []);
    angular.module('app.services', []);

    var app = angular.module('app', [
        'ngRoute',
        'ui.router',
        'cgBusy',
        'app.controllers',
        'app.services'
    ]);

    angular.module('app.services')
        .constant('endpoints.config', {
            apiBaseUrl: 'http://localhost:4457/api',
        });

    app.run(['$log', '$rootScope',
        function ($log, $rootScope) {
            $rootScope.$log = $log;
            console.debug('app run.');
        }]);

}())