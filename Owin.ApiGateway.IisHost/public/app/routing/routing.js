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
            configuration: null,
        };

        vm.startRouteEditing = startRouteEditing;

        activate();

        function activate() {
            vm.isBusy = true;

            var promises = [];
            promises.push(getCurrentConfiguration());

            common.activateController(promises, controllerId)
               .then(function () {
                   // setView();
               })
               .catch(function () {
               })
               .then(function () {
                   vm.isBusy = false;
               });
        }

        function getCurrentConfiguration() {

            vm.modelPromise = context.getCurrentConfiguration()
                .then(function (response) {

                    if (response.status === 200) {

                        vm.model.configuration = response.data;

                    } else {
                        logError("Sorry, something went wrong during...");
                        vm.model = null;
                    }

                    return vm.model;
                });

            return common.$q.when(vm.modelPromise);
        }

        function startRouteEditing(route) {

        }
    }
})();