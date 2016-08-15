(function () {
    "use strict";

    var controllerId = "endpointsController";
    angular
        .module("app-owinapigateway-admin")
        .controller("endpointsController", endpointsController);

    endpointsController.$inject = ["$scope", "common", "context", "config"];

    function endpointsController($scope, common, context, config) {

        var vm = this;

        vm.model = {

        };

        vm.getStatusText = getStatusText;
        vm.getSubstatusText = getSubstatusText;
        vm.startEndpointEditing = startEndpointEditing;

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

        function startEndpointEditing(endpoint) {
            vm.editedEndpoint = endpoint;
        }

        function getSubstatusText(endpoint) {
            var ins = endpoint.instances.instance;
            var isAtLeastOneDown = false;
            var isAtLeastOneUnknown = false;

            if (ins) {
                for (var i = 0; i < ins.length; i++) {
                    var instance = ins[i];

                    if (instance.status == 0) {
                        isAtLeastOneUnknown = true;
                    }
                    if (instance.status == 2) {
                        isAtLeastOneDown = true;
                    }
                }
            }

            var subStatusText = "";
            if (isAtLeastOneDown) {
                subStatusText += "[down]";
            }
            if (isAtLeastOneUnknown) {
                subStatusText += "[unknown]";
            }

            return subStatusText;
        }

        function getStatusText(endpoint) {
            var ins = endpoint.instances.instance;
            var isAtLeastOneUp = false;
            if (ins) {
                for (var i = 0; i < ins.length; i++) {
                    var instance = ins[i];

                    if (instance.status == 1) {
                        isAtLeastOneUp = true;
                    }
                }
            }

            return isAtLeastOneUp ? "OK" : "NOT OK";
        }

        function getCurrentConfiguration() {

            vm.modelPromise = context.getCurrentConfiguration()
                .then(function (response) {

                    if (response.status === 200) {

                        vm.model = response.data;

                    } else {
                        logError("Sorry, something went wrong during...");
                        vm.model = null;
                    }

                    return vm.model;
                });

            return common.$q.when(vm.modelPromise);
        }
    }
})();