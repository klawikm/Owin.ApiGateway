(function () {
    'use strict';

    var serviceId = 'context';
    angular
        .module('app-owinapigateway-admin')
        .factory(serviceId, context);

    context.$inject = ['$http', 'common', 'config'];

    function context($http, common, config) {
        var $q = common.$q;

        var service = {
            getCurrentConfiguration: getCurrentConfiguration
        };

        function getCurrentConfiguration() {

            var promise = $http.get(config.getCurrentConfigurationUrl)
                .then(function (response) {
                    return response;
                }, function (error) {
                    return error;
                });

            return $q.when(promise);
        }

        return service;
    }
})();