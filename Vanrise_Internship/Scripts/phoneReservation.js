var app = angular.module('reservationApp', ['ui.bootstrap']);

// Define the controller
app.controller('ReservationController', function ($scope, $http) {
    console.log("ReservationController initialized");
    $scope.filteredReservations = [];
    $scope.selectedPhoneNumber = null;
    $scope.selectedClientID = null;

    $scope.$watch('selectedClientID', function (newVal, oldVal) {
        console.log("Controller $watch - selectedClientID changed. Old:", oldVal, "New:", newVal);
    });

    // Load reservations from the database (all initially)
    $scope.loadReservations = function () {
        $http.get('/reservations/GetAllReservations')
            .then(function (response) {
                $scope.reservations = response.data;
                $scope.filteredReservations = $scope.reservations;
            }, function (error) {
                console.error('Error loading reservations:', error);
                alert('An error occurred while loading reservations.');
            });
    };

    // Filter reservations by phone number
    $scope.filterByPhoneNumber = function () {
        console.log("Phone number filter triggered.");
        console.log("Selected phone number:", $scope.selectedPhoneNumber);

        if ($scope.selectedPhoneNumber) {
            console.log("Fetching reservations for phone number ID:", $scope.selectedPhoneNumber);
            $http.get('/reservations/GetReservationsByPhoneNumber', {
                params: { phoneNumberID: $scope.selectedPhoneNumber }
            }).then(function (response) {
                console.log("Response received from /reservations/GetReservationsByPhoneNumber:", response);
                $scope.filteredReservations = response.data;
                console.log("Filtered reservations by phone number:", $scope.filteredReservations);
            }, function (error) {
                console.error('Error filtering reservations by phone number:', error);
                alert('An error occurred while filtering reservations.');
            });
        } else {
            console.log("No phone number selected. Showing all reservations.");
            $scope.filteredReservations = $scope.reservations;  // Show all if no phone number is selected
            console.log("All reservations:", $scope.filteredReservations);
        }
    };

    // Filter reservations by client name
    $scope.filterByClientName = function () {
        console.log("Client name filter triggered.");
        console.log("Selected client ID:", $scope.selectedClientID);

        if ($scope.selectedClientID) {
            console.log("Fetching reservations for client ID:", $scope.selectedClientID);
            $http.get('/reservations/GetReservationsByClient', {
                params: { clientID: $scope.selectedClientID }
            }).then(function (response) {
                console.log("Response received from /reservations/GetReservationsByClient:", response);
                $scope.filteredReservations = response.data;
                console.log("Filtered reservations by client name:", $scope.filteredReservations);
            }, function (error) {
                console.error('Error filtering reservations by client name:', error);
                alert('An error occurred while filtering reservations.');
            });
        } else {
            console.log("No client selected. Showing all reservations.");
            $scope.filteredReservations = $scope.reservations;  // Show all if no client is selected
            console.log("All reservations:", $scope.filteredReservations);
        }
    };






    // Load reservations when the page loads
    $scope.loadReservations();
});





// Phone Number Selector Directive
app.directive('phoneNumberSelector', function ($http) {
    return {
        restrict: 'E',
        scope: {
            selected: '='  // Two-way binding
        },
        template: `
            <select class="form-control" ng-model="selected" ng-options="phone.ID as phone.Number for phone in phoneNumbers">
                <option value="">Select Phone Number</option>
            </select>
        `,
        controller: function ($scope) {
            // Function to load phone numbers from the server
            $scope.getData = function () {
                $http.get('/api/PhoneNumbers/GetAllPhoneNumbers')
                    .then(function (response) {
                        console.log(response.data);  // Log to check if phone numbers are returned correctly
                        $scope.phoneNumbers = response.data;  // Set the phone numbers
                    }, function (error) {
                        console.error('Error loading phone numbers:', error);
                    });
            };

            // Load data when the directive is initialized
            $scope.getData();
        }
    };
});



app.directive('clientNameSelector', function ($http) {
    return {
        restrict: 'E',
        scope: {
            selected: '='  // Two-way binding for selected client ID
        },
        template: `
            <select class="form-control" ng-model="selected" ng-options="client.ClientID as client.Name for client in clients" ng-change="onClientSelected()">
                <option value="">Select Client Name</option>
            </select>
        `,
        controller: function ($scope) {
            console.log("clientNameSelector directive initialized");

            $scope.getData = function () {
                console.log("getData function called");
                $http.get('/clients/GetAllClients')
                    .then(function (response) {
                        console.log("Clients data received:", response.data);
                        $scope.clients = response.data;
                        console.log("$scope.clients set:", $scope.clients);
                    }, function (error) {
                        console.error('Error loading clients:', error);
                    });
            };

            $scope.onClientSelected = function () {
                console.log("Client selected in directive:", $scope.selected);
            };

            // Fetch client data when the directive is initialized
            $scope.getData();

            // Watch the selected model for any changes
            $scope.$watch('selected', function (newVal, oldVal) {
                console.log("$watch triggered in directive. Old value:", oldVal, "New value:", newVal);
                if (newVal) {
                    console.log("Selected client ID changed to:", newVal);
                } else {
                    console.log("No client selected in directive.");
                }
            });

            // Log the initial state of selected
            console.log("Initial state of selected in directive:", $scope.selected);
        },
        link: function (scope, element, attrs) {
            console.log("clientNameSelector link function executed");
            console.log("selected attribute:", attrs.selected);
        }
    };
});
