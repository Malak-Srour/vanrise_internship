var app = angular.module('phoneApp', ['ui.bootstrap']);

app.controller('PhoneNumberController', function ($scope, $http, $uibModal) {
    $scope.phoneNumbers = [];
    $scope.filteredPhoneNumbers = [];

    // Load phone numbers
    $scope.loadPhoneNumbers = function () {
        $http.get('/api/PhoneNumbers/GetAllPhoneNumbers').then(function (response) {
            $scope.phoneNumbers = response.data;
            $scope.filteredPhoneNumbers = $scope.phoneNumbers;
        }, function (error) {
            console.error('Error loading phone numbers:', error);
        });
    };

    $scope.loadPhoneNumbers();

    // Search function
    $scope.search = function () {
        const params = {
            number: $scope.searchText,
            deviceId: $scope.selectedDeviceId  // If deviceId is included in search
        };

        $http.get('/api/PhoneNumbers/SearchPhoneNumbers', { params: params })
            .then(function (response) {
                $scope.filteredPhoneNumbers = response.data;
            }, function (error) {
                console.error('Error filtering phone numbers:', error);
                alert('An error occurred while filtering phone numbers.');
            });
    };

    // Search by Device (triggered by device-selector)
    $scope.searchByDevice = function () {
        if ($scope.selectedDeviceId) {
            const params = {
                deviceId: $scope.selectedDeviceId  // Filter by selected device
            };

            $http.get('/api/PhoneNumbers/SearchPhoneNumbersByDevice', { params: params })
                .then(function (response) {
                    $scope.filteredPhoneNumbers = response.data;
                }, function (error) {
                    console.error('Error filtering phone numbers by device:', error);
                    alert('An error occurred while filtering phone numbers by device.');
                });
        } else {
            // Reset to show all phone numbers if no device is selected
            $scope.filteredPhoneNumbers = $scope.phoneNumbers;
        }
    };

    // Call the load function on initialization
    $scope.loadPhoneNumbers();


    // Delete function
    $scope.deletePhoneNumber = function (id) {
        if (confirm('Are you sure you want to delete this phone number?')) {
            $http.delete('/api/PhoneNumbers/DeletePhoneNumber?id=' + id).then(function () {
                $scope.filteredPhoneNumbers = $scope.filteredPhoneNumbers.filter(function (phoneNumber) {
                    return phoneNumber.ID !== id;
                });
                $scope.search(); // Update the displayed phone numbers
            }, function (error) {
                console.error('Error deleting phone number:', error);
                alert('Failed to delete the phone number. Please try again.');
            });
        }
    };

    // Function to open the Add Phone Number Modal
    $scope.openAddModal = function () {
        var modalInstance = $uibModal.open({
            template: document.getElementById('addPhoneNumberModal.html').innerHTML,
            controller: 'AddPhoneNumberModalController',
            resolve: {
                newPhoneNumber: function () {
                    return {};
                }
            }
        });

        modalInstance.result.then(function (newPhoneNumber) {
            if (/^\d{8}$/.test(newPhoneNumber.Number)) {
                $scope.newPhoneNumber = newPhoneNumber;
                $scope.AddPhoneNumber();
            } else {
                alert("The phone number must be exactly 8 digits, and it should contain only numbers.");
            }
        }, function () {
            console.log('Modal dismissed at: ' + new Date());
        });
    };

    // Function to add a new phone number
    $scope.AddPhoneNumber = function () {
        $http.post('/api/PhoneNumbers/AddPhoneNumber', $scope.newPhoneNumber)
            .then(function (response) {
                // Phone number added successfully, reload the list
                $scope.loadPhoneNumbers();
            }, function (error) {
                // Handle the error, including duplicate number validation
                if (error.status === 400 && error.data === "This phone number is already in use.") {
                    alert("This phone number is already in use. Please enter a different number.");
                } else {
                    console.error('Error adding phone number:', error);
                    alert("An error occurred while adding the phone number. Please try again.");
                }
            });
    };

    // Function to open the Edit Phone Number Modal
    $scope.openEditModal = function (phoneNumber) {
        var modalInstance = $uibModal.open({
            template: document.getElementById('editPhoneNumberModal.html').innerHTML,
            controller: 'EditPhoneNumberModalController',
            resolve: {
                selectedPhoneNumber: function () {
                    return angular.copy(phoneNumber);
                }
            }
        });

        modalInstance.result.then(function (updatedPhoneNumber) {
            $scope.updatePhoneNumber(updatedPhoneNumber);
        }, function () {
            console.log('Modal dismissed at: ' + new Date());
        });
    };

    // Function to update a phone number
    $scope.updatePhoneNumber = function (updatedPhoneNumber) {
        $http.put('/api/PhoneNumbers/UpdatePhoneNumber', updatedPhoneNumber)
            .then(function (response) {
                $scope.loadPhoneNumbers(); // Refresh the phone number list after updating
            }, function (error) {
                console.error('Error updating phone number:', error);
                alert('An error occurred while updating the phone number. Please try again.');
            });
    };
});

// Controller for Add Phone Number Modal
app.controller('AddPhoneNumberModalController', function ($scope, $uibModalInstance, $http) {
    $scope.newPhoneNumber = {};
    $scope.numberExists = false;

    // Watch for changes in the phone number input field and reset the flag
    $scope.$watch('newPhoneNumber.Number', function (newVal, oldVal) {
        if (newVal !== oldVal) {
            $scope.numberExists = false;
        }
    });

    $scope.saveNewPhoneNumber = function () {
        // Check if the phone number already exists before saving
        $http.get('/api/PhoneNumbers/GetAllPhoneNumbers').then(function (response) {
            var phoneNumbers = response.data;
            var duplicate = phoneNumbers.some(function (phone) {
                return phone.Number === $scope.newPhoneNumber.Number;
            });

            if (duplicate) {
                $scope.numberExists = true; // Show the validation message
            } else {
                $scope.numberExists = false;
                $uibModalInstance.close($scope.newPhoneNumber);
            }
        });
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    };
});

// Controller for Edit Phone Number Modal
app.controller('EditPhoneNumberModalController', function ($scope, $uibModalInstance, $http, selectedPhoneNumber) {
    $scope.selectedPhoneNumber = selectedPhoneNumber || {};
    $scope.numberExists = false;

    // Watch for changes in the phone number input field and reset the flag
    $scope.$watch('selectedPhoneNumber.Number', function (newVal, oldVal) {
        if (newVal !== oldVal) {
            $scope.numberExists = false;
        }
    });

    // Function to save the edited phone number
    $scope.saveEditedPhoneNumber = function () {
        // Check if the phone number already exists before saving
        $http.get('/api/PhoneNumbers/GetAllPhoneNumbers').then(function (response) {
            var phoneNumbers = response.data;
            var duplicate = phoneNumbers.some(function (phone) {
                return phone.Number === $scope.selectedPhoneNumber.Number && phone.ID !== $scope.selectedPhoneNumber.ID;
            });

            if (duplicate) {
                $scope.numberExists = true; // Show the validation message
            } else if (!/^\d{8}$/.test($scope.selectedPhoneNumber.Number)) {
                alert("The phone number must be exactly 8 digits, and it should contain only numbers.");
            } else {
                $scope.numberExists = false;
                $uibModalInstance.close($scope.selectedPhoneNumber);
            }
        });
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    };
});

// Directive for the device selector
app.directive('deviceSelector', function ($http) {
    return {
        restrict: 'E',
        scope: {
            ngModel: '=',  // Bind the selected device ID to the ngModel attribute
            ngChange: '&'  // Add this to trigger the change event
        },
        template: `
                <select class="form-control" ng-model="ngModel" ng-change="ngChange()" ng-options="device.ID as device.Name for device in devices">
                    <option value="">Select a Device</option>
                </select>
            `,
        link: function (scope, element, attrs) {
            scope.devices = [];

            // Load the devices from the API
            $http.get('/api/Devices/GetAllDevices').then(function (response) {
                scope.devices = response.data;
            }, function (error) {
                console.error('Error loading devices:', error);
            });
        }
    };
});