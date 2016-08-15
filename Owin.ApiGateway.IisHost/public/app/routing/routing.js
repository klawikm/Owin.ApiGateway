(function () {
    "use strict";

    var controllerId = "routingController";
    angular
        .module("app-owinapigateway-admin")
        .controller("routingController", routingController);

    routingController.$inject = ["$scope", "common", "context", "config"];

    function routingController($scope, common, context, config) {

        var vm = this;

        vm.model = {

        };

        activate();

        function activate() {
            vm.isBusy = true;
        }
    }
})();