app.directive('phoneNumberSelector', function () {
    return {
        restrict: 'E',
        scope: {
            selected: '='
        },
        template: `
            <select class="form-control" ng-model="selected" ng-options="phone.Number for phone in phoneNumbers track by phone.ID">
                <option value="">Select Phone Number</option>
            </select>
        `,
        controller: function ($scope, $http) {
            // Load phone numbers from server
            $http.get('/phones/GetAllPhoneNumbers')
                .then(function (response) {
                    $scope.phoneNumbers = response.data;
                }, function (error) {
                    console.error('Error loading phone numbers:', error);
                });
        }
    };
});
