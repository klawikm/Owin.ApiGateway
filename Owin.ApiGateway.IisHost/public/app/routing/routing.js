﻿(function () {
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

        vm.availableConditionTypes = [{ code: "SoapAction", name: "Soap Action" }, { code: "RequestPathAndQuery", name: "RequestPath RegEx" }, { code: "AlwaysMatching", name: "Always Matching" }];

        vm.startRouteEditing = startRouteEditing;
        vm.cancelRouteEditing = cancelRouteEditing;
        vm.updateEditedRoute = updateEditedRoute;
        vm.addCacheSection = addCacheSection;
        vm.addLoggerSection = addLoggerSection;
        vm.addNewRouting = addNewRouting;
        vm.deleteRoute = deleteRoute;

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
            vm.editedRoute = route;

            setPropertiesDescribingCondition(route, vm);
        }

        function deleteRoute(route) {
            var index = vm.model.configuration.routes.indexOf(route);
            if (index != -1) {
                vm.model.configuration.routes.splice(index, 1);
                udateConfiguration(vm.model.configuration);
            }
        }

        function addNewRouting() {
            var newRoute = {};
            vm.editedRoute = newRoute;
        }

        function setPropertiesDescribingCondition(route, obj) {
            if (route.soapActionCondition) {
                obj.selectedConditionType = "SoapAction";
                obj.selectedConditionParameter = route.soapActionCondition.requiredSoapAction;
            } else if (route.requestPathAndQueryCondition) {
                obj.selectedConditionType = "RequestPathAndQuery";
                obj.selectedConditionParameter = route.requestPathAndQueryCondition.requestPathRegexString;
            } else if (route.alwaysMatchingCondition) {
                obj.selectedConditionType = "AlwaysMatching";
                obj.selectedConditionParameter = null;
            }
        }

        function cancelRouteEditing() {
            vm.editedRoute = null;
        }

        function addCacheSection() {
            vm.editedRoute.cache = {};
        }

        function addLoggerSection() {
            vm.editedRoute.logger = {};
        }

        function updateEditedRoute() {
            var prevConditionDescr = {};
            setPropertiesDescribingCondition(vm.editedRoute, prevConditionDescr);

            if (prevConditionDescr.selectedConditionType != vm.selectedConditionType) {
                vm.editedRoute.soapActionCondition = null;
                vm.editedRoute.requestPathAndQueryCondition = null;
                vm.editedRoute.alwaysMatchingCondition = null;

                switch (vm.selectedConditionType) {
                    case "SoapAction":
                        vm.editedRoute.soapActionCondition = { };
                        break;
                    case "RequestPathAndQuery":
                        vm.editedRoute.requestPathAndQueryCondition = { };
                        break;
                    case "AlwaysMatching":
                        vm.editedRoute.alwaysMatchingCondition = { };
                        break;
                }
            }

            switch (vm.selectedConditionType) {
                case "SoapAction":
                    vm.editedRoute.soapActionCondition.requiredSoapAction = vm.selectedConditionParameter;
                    break;
                case "RequestPathAndQuery":
                    vm.editedRoute.requestPathAndQueryCondition.requestPathRegexString = vm.selectedConditionParameter;
                    break;
                case "AlwaysMatching":
                    break;
            }

            // add new route to the configuration
            if (vm.model.configuration.routes == null) {
                vm.model.configuration.routes = [];
            }

            if (vm.model.configuration.routes.indexOf(vm.editedRoute) == -1) {
                vm.model.configuration.routes.push(vm.editedRoute);
            }

            udateConfiguration(vm.model.configuration);
        }

        function udateConfiguration(configuration) {
            context.updateCurrentConfiguration(configuration).then(function (response) {
                if (response.status === 200) {
                    cancelRouteEditing();
                } else {
                    logError("Sorry, something went wrong during...");
                }
            });
        }
    }
})();