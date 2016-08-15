(function () {
    'use strict';

    var app = angular.module('app-owinapigateway-admin');

    // Collect the routes
    app.constant('routes', getRoutes());
    
    // Configure the routes and route resolvers
    app.config(['$routeProvider', 'routes', routeConfigurator]);
    function routeConfigurator($routeProvider, routes) {

        routes.forEach(function (r) {
            $routeProvider.when(r.url, r.config);
        });
        $routeProvider.otherwise({ redirectTo: '/' });
    }

    // Define the routes 
    function getRoutes() {
        return [
            {
                url: '/',
                config: {
                    title: 'dashboard',
                    templateUrl: '/Owin.ApiGateway.IisHost/admin/app/dashboard/dashboard.html',
                    controller: 'dashboardController',
                    controllerAs: 'vm'
                }
            },
            {
                url: '/endpoints',
                config: {
                    title: 'endpoints',
                    templateUrl: '/Owin.ApiGateway.IisHost/admin/app/endpoints/endpoints.html',
                    controller: 'endpointsController',
                    controllerAs: 'vm'
                }
            },
            {
                url: '/routing',
                config: {
                    title: 'routing',
                    templateUrl: '/Owin.ApiGateway.IisHost/admin/app/routing/routing.html',
                    controller: 'routingController',
                    controllerAs: 'vm'
                }
            }
        ];
    }
})();