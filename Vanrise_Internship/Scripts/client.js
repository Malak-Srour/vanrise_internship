// Define the Angular app
var app = angular.module('clientApp', ['ui.bootstrap']);

// Define the controller
app.controller('ClientController', function ($scope, $http, $uibModal) {
    $scope.clients = [];
    $scope.filteredClients = [];
    $scope.clientTypeFilter = '';
    $scope.showBirthDate = false;
    $scope.isEdit = false;

    // Load all clients with reservation status
    $scope.loadClients = function () {
        $http.get('/clients/GetAllClients')
            .then(function (response) {
                $scope.clients = response.data;

                // Check if the client has active reservations
                $scope.clients.forEach(function (client) {
                    $http.get('/reservations/GetReservationsByClient', { params: { clientID: client.ID } })
                        .then(function (res) {
                            client.hasActiveReservation = res.data.some(function (reservation) {
                                return reservation.EED === null;  // Active reservation (EED is null)
                            });
                        }, function (error) {
                            console.error('Error checking reservation status:', error);
                        });
                });

                $scope.filteredClients = $scope.clients;
            }, function (error) {
                console.error('Error loading clients:', error);
                alert('An error occurred while loading clients.');
            });
    };



    // Search function by name
    $scope.search = function () {
        if (!$scope.searchText) {
            $scope.filteredClients = $scope.clients;
            return;
        }
        $http.get('/Clients/GetFilteredClients', { params: { searchTerm: $scope.searchText } })
            .then(function (response) {
                $scope.filteredClients = response.data;
            }, function (error) {
                console.error('Error filtering clients:', error);
                alert('An error occurred while filtering clients.');
            });
    };

    // Search function by client type
    $scope.searchByType = function () {
        var selectedType = $scope.clientTypeFilter;
        if (!selectedType) {
            $scope.loadClients();
            return;
        }
        $http.get('/clients/GetFilteredByType', { params: { clientType: selectedType } })
            .then(function (response) {
                $scope.filteredClients = response.data;
            }, function (error) {
                console.error('Error filtering clients by type:', error);
                alert('An error occurred while filtering clients by type.');
            });
    };


    // Delete client
    $scope.deleteClient = function (id) {
        if (confirm('Are you sure you want to delete this client?')) {
            $http.delete('/clients/DeleteClient/' + id)
                .then(function (response) {
                    // Refresh the client list after deletion
                    $scope.loadClients();
                }, function (error) {
                    console.error('Error deleting client:', error);
                    alert('An error occurred while deleting the client.');
                });

        }
    };

    // Open Add Client Modal
    $scope.openAddModal = function () {
        $scope.isEdit = false;
        $scope.selectedClient = {};
        $scope.showBirthDate = false;
        $scope.nameExists = false;
        var modalInstance = $uibModal.open({
            templateUrl: 'clientModal.html',
            controller: 'ModalInstanceCtrl',
            scope: $scope
        });
    };

    // Open Edit Client Modal
    $scope.openEditModal = function (client) {
        $scope.isEdit = true;
        $scope.selectedClient = angular.copy(client);

        if ($scope.selectedClient.BirthDate) {
            $scope.selectedClient.BirthDate = new Date($scope.selectedClient.BirthDate).toISOString().split('T')[0];
        }

        $scope.showBirthDate = $scope.selectedClient.Type == 1;
        $scope.nameExists = false;

        var modalInstance = $uibModal.open({
            templateUrl: 'clientModal.html',
            controller: 'ModalInstanceCtrl',
            scope: $scope
        });
    };

    // Open Reserve Modal
    $scope.openReserveModal = function (client) {
        $scope.client = client;
        $scope.selectedPhoneNumber = null;  // Initialize

        var modalInstance = $uibModal.open({
            templateUrl: 'reservePhoneNumberModal.html',
            controller: 'ReservePhoneNumberController',  // New controller for reserve modal
            scope: $scope  // Share the scope with the modal
        });

        modalInstance.result.then(function (selectedPhoneNumber) {
            // Save the reservation
            if (!client.ID || !selectedPhoneNumber) {
                console.error("Client ID or Phone Number is missing.");
                return;
            }

            // Make the API call to save the reservation
            $http.post('/reservations/ReservePhoneNumber', {
                ClientID: client.ID,
                PhoneNumberID: selectedPhoneNumber,
                BED: new Date(),
                EED: null
            }).then(function (response) {
                alert('Phone number reserved successfully!');
            }, function (error) {
                console.error('Error reserving phone number:', error);
            });
        });
    };





    // Open Unreserve Modal
    $scope.openUnreserveModal = function (client) {
        $scope.client = client;
        $scope.selectedPhoneNumber = null;

        // Load reserved phone numbers for the client
        $http.get('/reservations/GetReservedPhoneNumbers', { params: { clientId: client.ID } })
            .then(function (response) {
                $scope.reservedPhoneNumbers = response.data;
            }, function (error) {
                console.error('Error loading reserved phone numbers:', error);
            });

        var modalInstance = $uibModal.open({
            templateUrl: 'unreservePhoneNumberModal.html',
            controller: 'ReserveModalInstanceCtrl',
            scope: $scope
        });

        modalInstance.result.then(function () {
            // Unreserve the selected phone number
            $http.post('/reservations/UnreservePhoneNumber', {
                ClientID: client.ID,
                PhoneNumberID: $scope.selectedPhoneNumber.ID
            }).then(function (response) {
                alert('Phone number unreserved successfully!');
            }, function (error) {
                console.error('Error unreserving phone number:', error);
            });
        });
    };




    


    // Load clients when the page loads
    $scope.loadClients();
});

// Controller for the reserve phone number modal
app.controller('ReservePhoneNumberController', function ($scope, $uibModalInstance) {

    // Function to reserve the phone number
    $scope.reservePhoneNumber = function () {
        var selectedPhoneNumber = $scope.getData();  // Get selected phone number from directive

        if (!selectedPhoneNumber) {
            alert("Please select a phone number to reserve.");
            return;
        }

        console.log("Reserving phone number with ID:", selectedPhoneNumber);  // Log the selected phone number ID

        // Make the API call to reserve the phone number
        $http.post('/reservations/ReservePhoneNumber', {
            ClientID: $scope.client.ID,
            PhoneNumberID: selectedPhoneNumber  // Pass the selected phone number ID
        }).then(function (response) {
            alert('Phone number reserved successfully!');
            $uibModalInstance.close();  // Close the modal upon success
        }, function (error) {
            console.error('Error reserving phone number:', error);
            alert('Failed to reserve the phone number. Please try again.');
        });
    };

    // Cancel the modal
    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
        console.log("Modal canceled.");
    };
});




app.controller('ReserveModalInstanceCtrl', function ($scope, $uibModalInstance, $http) {
    // Log selected phone number whenever it changes
    $scope.logSelectedPhoneNumber = function () {
        console.log("Selected phone number:", $scope.selectedPhoneNumber);
    };

    $scope.unreservePhoneNumber = function () {
        if (!$scope.selectedPhoneNumber) {
            alert("Please select a phone number to unreserve.");
            return;
        }

        console.log("Unreserving phone number with ID:", $scope.selectedPhoneNumber.ID); // Log ID

        // Make the API call to unreserve the phone number
        $http.post('/reservations/UnreservePhoneNumber', {
            ClientID: $scope.client.ID,
            PhoneNumberID: $scope.selectedPhoneNumber.ID  // Log the selected phone number ID
        }).then(function (response) {
            alert('Phone number unreserved successfully!');
            $uibModalInstance.close();  // Close the modal upon success
        }, function (error) {
            console.error('Error unreserving phone number:', error);
            alert('Failed to unreserve the phone number. Please try again.');
        });
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    };
});






// Modal Controller (shared for Add and Edit)
app.controller('ModalInstanceCtrl', function ($scope, $http, $uibModalInstance) {
    $scope.nameExists = false;
    $scope.ageInvalid = false;

    // Watch for changes in the Name field and reset nameExists flag
    $scope.$watch('selectedClient.Name', function (newVal, oldVal) {
        if (newVal !== oldVal) {
            $scope.nameExists = false;  // Reset flag when name is changed
        }
    });

    // Toggle BirthDate field based on Type
    $scope.toggleBirthDate = function () {
        $scope.showBirthDate = $scope.selectedClient.Type == 1;  // Show birthdate only for Individual clients
    };

    // Validate that age is greater than 18 for individuals
    $scope.validateAge = function () {
        if ($scope.selectedClient.Type == 1 && $scope.selectedClient.BirthDate) {
            var birthDate = new Date($scope.selectedClient.BirthDate);
            var age = calculateAge(birthDate);
            $scope.ageInvalid = age < 18;
        } else {
            $scope.ageInvalid = false;  // No age validation for organizations
        }
    };

    // Function to calculate age based on the birthdate
    function calculateAge(birthDate) {
        var today = new Date();
        var age = today.getFullYear() - birthDate.getFullYear();
        var monthDifference = today.getMonth() - birthDate.getMonth();
        if (monthDifference < 0 || (monthDifference === 0 && today.getDate() < birthDate.getDate())) {
            age--;
        }
        return age;
    }

    // Save the client (Add or Edit)
    $scope.saveClient = function () {
        // Check if the name already exists for other clients (excluding the current one)
        if ($scope.clients.some(function (client) {
            return client.Name.toLowerCase() === $scope.selectedClient.Name.toLowerCase() && client.ID !== $scope.selectedClient.ID;
        })) {
            $scope.nameExists = true;  // Name already exists
            return;
        }

        // Reset error message if the name is unique
        $scope.nameExists = false;

        if (!$scope.isEdit) {
            // Add new client
            $http.post('/clients/AddClient', $scope.selectedClient)
                .then(function (response) {
                    $scope.loadClients();  // Reload clients after saving
                    $uibModalInstance.close();  // Close modal
                }, function (error) {
                    console.error('Error adding client:', error);
                    alert('An error occurred while adding the client.');
                });
        } else {
            // Edit existing client
            $http.put('/clients/UpdateClient', $scope.selectedClient)
                .then(function (response) {
                    $scope.loadClients();  // Reload clients after updating
                    $uibModalInstance.close();  // Close modal
                }, function (error) {
                    console.error('Error updating client:', error);
                    alert('An error occurred while updating the client.');
                });
        }
    };

    // Cancel the modal
    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    };
});

app.directive('phoneNumberSelector', function ($http) {
    return {
        restrict: 'E',
        scope: {
            selected: '='  // Two-way binding to pass selected phone number
        },
        template: `
            <div>
                <select class="form-control" ng-model="selected" ng-options="phone.ID as phone.Number for phone in phoneNumbers">
                    <option value="">Select Phone Number</option>
                </select>
            </div>
        `,
        controller: function ($scope) {
            // Fetch available phone numbers from the server
            $scope.phoneNumbers = [];

            $http.get('/api/PhoneNumbers/GetAllPhoneNumbers')
                .then(function (response) {
                    $scope.phoneNumbers = response.data;  // Set phone numbers to the dropdown
                }, function (error) {
                    console.error('Error fetching phone numbers:', error);
                });

            // A function to get the selected data
            $scope.getData = function () {
                return $scope.selected;  // Return the selected phone number ID
            };
        }
    };
});



