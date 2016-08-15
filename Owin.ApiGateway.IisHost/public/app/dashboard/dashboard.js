(function () {
    "use strict";

    var controllerId = "dashboardController";
    angular
        .module("app-owinapigateway-admin")
        .controller("dashboardController", dashboardController);

    dashboardController.$inject = ["$scope", "common", "context", "config"];

    function dashboardController($scope, common, context, config) {

        var vm = this;

        vm.model = {
           
        };
        
        activate();

        function activate() {
            vm.isBusy = true;
        }
    }
})();