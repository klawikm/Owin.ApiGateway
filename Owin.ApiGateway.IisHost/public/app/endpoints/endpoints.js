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
            configuration: null,
            availableStatuses: [{ code: 0, name: "Unknown" }, { code: 1, name: "Up" }, { code: 2, name: "Down" } ],
        };

        vm.getStatusText = getStatusText;
        vm.getSubstatusText = getSubstatusText;
        vm.startEndpointEditing = startEndpointEditing;
        vm.cancelEndpointEditing = cancelEndpointEditing;
        vm.updatedEditedEndpoint = updatedEditedEndpoint;
        vm.addNewEndpoint = addNewEndpoint;
        vm.deleteEndpoint = deleteEndpoint;
        vm.deleteInstance = deleteInstance;
        vm.addNewInstance = addNewInstance;
        
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

        function deleteInstance(endpoint, instanceToDel) {
            var index = endpoint.instances.instance.indexOf(instanceToDel);
            if (index != -1) {
                endpoint.instances.instance.splice(index, 1);
            }
        }

        function addNewInstance(endpoint) {

            if (endpoint.instances == null) {
                endpoint.instances = {};
            }

            if (endpoint.instances.instance == null) {
                endpoint.instances.instance = [];
            }

            endpoint.instances.instance.push({});
        }

        function addNewEndpoint() {
            var newEndpoint = {};
            vm.editedEndpoint = newEndpoint;
        }

        function deleteEndpoint(endpoint) {
            var index = vm.model.configuration.endpoints.indexOf(endpoint);
            if (index != -1) {
                vm.model.configuration.endpoints.splice(index, 1);
                udateConfiguration(vm.model.configuration);
            }
        }

        function updatedEditedEndpoint() {
            // add new endpoint to the configuration
            if (vm.model.configuration.endpoints == null) {
                vm.model.configuration.endpoints = [];
            }

            if (vm.model.configuration.endpoints.indexOf(vm.editedEndpoint) == -1) {
                vm.model.configuration.endpoints.push(vm.editedEndpoint);
            }

            udateConfiguration(vm.model.configuration);
        }

        function udateConfiguration(configuration) {
            context.updateCurrentConfiguration(configuration).then(function (response) {
                if (response.status === 200) {
                    cancelEndpointEditing();
                } else {
                    logError("Sorry, something went wrong during...");
                }
            });
        }

        function cancelEndpointEditing() {
            vm.editedEndpoint = null;
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

                        vm.model.configuration = response.data;

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