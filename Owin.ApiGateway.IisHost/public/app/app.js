
(function () {
    'use strict';

    var app = angular.module('app-owinapigateway-admin', [
        'ngRoute',          // routing
         // Custom modules 
        'common',           // common functions
    ]);

 /**
 * A generic confirmation for risky actions.
 * Usage: Add attributes: ng-really-message="Are you sure"? ng-really-click="takeAction()" function
 */
    angular.module('app-owinapigateway-admin').directive('ngReallyClick', [function () {
        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                element.bind('click', function () {
                    var message = attrs.ngReallyMessage;
                    if (message && confirm(message)) {
                        scope.$apply(attrs.ngReallyClick);
                    }
                });
            }
        }
    }]);


})();