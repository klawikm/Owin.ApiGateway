(function () {
    'use strict';

    var app = angular.module('app-owinapigateway-admin');

    var config = {
        getCurrentConfigurationUrl: '/Owin.ApiGateway.IisHost/api/Configuration/GetCurrentConfiguration',
        version: '1.0.0'
    };

    app.value('config', config);
})();