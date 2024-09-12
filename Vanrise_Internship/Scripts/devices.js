var app = angular.module('deviceApp', ['ui.bootstrap']);

app.controller('DeviceController', function ($scope, $http, $uibModal) {
    $scope.devices = [];
    $scope.filteredDevices = [];
    $scope.deviceLoadingError = '';
    $scope.isLoading = true;

    // Load devices
    $scope.loadDevices = function () {
        $scope.isLoading = true;
        console.log("Loading started", $scope.isLoading);

        // Simulate a delay of 3 seconds before hiding the loader
        setTimeout(function () {
            $http.get('/api/Devices/GetAllDevices').then(function (response) {
                $scope.devices = response.data;
                $scope.filteredDevices = $scope.devices;
                $scope.isLoading = false; // Stop the loader after the data is fetched
                console.log("Loading finished", $scope.isLoading);
            }, function (error) {
                $scope.deviceLoadingError = 'An error occurred while loading devices. Please try again.';
                $scope.isLoading = false; // Stop the loader in case of an error
                console.log("Loading error", $scope.isLoading);
            });

            // Apply the changes to the scope
            if (!$scope.$$phase) $scope.$apply();
        }, 3000); // Delay of 3 seconds before executing the HTTP request
    };





    $scope.loadDevices();

    // Search function
    $scope.search = function () {
        if (!$scope.searchText) {
            $scope.filteredDevices = $scope.devices; // Reset search
            return;
        }
        $http.get('/api/Devices/GetFilteredDevices', { params: { searchTerm: $scope.searchText } })
            .then(function (response) {
                $scope.filteredDevices = response.data;
            }, function (error) {
                console.error('Error filtering devices:', error);
                alert('An error occurred while filtering devices.');
            });
    };

    // Delete function
    $scope.deleteDevice = function (id) {
        if (confirm('Are you sure you want to delete this device?')) {
            $http.delete('/api/Devices/DeleteDevice?id=' + id).then(function () {
                $scope.filteredDevices = $scope.filteredDevices.filter(function (device) {
                    return device.ID !== id;
                });
                $scope.search(); // Update the displayed devices
            }, function (error) {
                console.error('Error deleting device:', error);
                alert('Failed to delete the device. Please try again.');
            });
        }
    };

    // Add Device Modal
    $scope.openAddModal = function () {
        var modalInstance = $uibModal.open({
            template: document.getElementById('addDeviceModal.html').innerHTML,
            controller: 'AddDeviceModalController',
            resolve: {
                newDevice: function () {
                    return {};
                }
            }
        });

        modalInstance.result.then(function (newDevice) {
            var existingDevice = $scope.devices.find(function (device) {
                return device.Name.toLowerCase() === newDevice.name.toLowerCase();
            });

            if (existingDevice) {
                alert("This name is already in use. Please choose a different name.");
            } else {
                $scope.newDevice = newDevice;
                $scope.addDevice();
            }
        }, function () {
            console.log('Modal dismissed at: ' + new Date());
        });
    };

    $scope.addDevice = function () {
        console.log('Adding device:', $scope.newDevice); // Debugging line
        $http.post('/api/Devices/AddDevice', $scope.newDevice)
            .then(function (response) {
                $scope.loadDevices(); // Refresh the device list after adding
            }, function (error) {
                console.error('Error adding device:', error);
                alert('An error occurred while adding the device. Please try again.');
            });
    };

    // Edit Device Modal
    $scope.openEditModal = function (device) {
        var modalInstance = $uibModal.open({
            template: document.getElementById('editDeviceModal.html').innerHTML,
            controller: 'EditDeviceModalController',
            resolve: {
                selectedDevice: function () {
                    return angular.copy(device);
                }
            }
        });

        modalInstance.result.then(function (updatedDevice) {
            // Check if the updated device name is already in use by another device
            var existingDevice = $scope.devices.find(function (d) {
                return d.Name.toLowerCase() === updatedDevice.Name.toLowerCase() && d.ID !== updatedDevice.ID;
            });

            if (existingDevice) {
                alert("This name is already in use. Please choose a different name.");
            } else {
                $scope.updateDevice(updatedDevice);
            }
        }, function () {
            console.log('Modal dismissed at: ' + new Date());
        });
    };

    $scope.updateDevice = function (updatedDevice) {
        console.log('Updating device:', updatedDevice); // Debugging line
        $http.put('/api/Devices/UpdateDevice', updatedDevice)
            .then(function (response) {
                $scope.loadDevices(); // Refresh the device list after updating
            }, function (error) {
                console.error('Error updating device:', error);
                alert('An error occurred while updating the device. Please try again.');
            });
    };

    // Drilldown for Phone Numbers Modal
    $scope.openPhoneNumberDrilldown = function (device) {
        var modalInstance = $uibModal.open({
            template: document.getElementById('phoneNumberDrilldown.html').innerHTML,
            controller: 'PhoneNumberDrilldownController',
            resolve: {
                selectedDevice: function () {
                    return angular.copy(device);
                }
            }
        });

        modalInstance.result.then(function () {
            // After closing the modal, refresh the devices
            $scope.loadDevices();
        }, function () {
            console.log('Phone number drilldown modal dismissed at: ' + new Date());
        });
    };

});

app.controller('AddDeviceModalController', function ($scope, $uibModalInstance, newDevice) {
    $scope.newDevice = newDevice || {};

    $scope.saveNewDevice = function (isValid) {
        if (isValid) {
            $uibModalInstance.close($scope.newDevice);
        } else {
            alert('Please fill in all required fields.');
        }
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    };
});

app.controller('EditDeviceModalController', function ($scope, $uibModalInstance, selectedDevice) {
    $scope.selectedDevice = selectedDevice || {};

    $scope.saveEditedDevice = function (isValid) {
        if (isValid) {
            $uibModalInstance.close($scope.selectedDevice);
        } else {
            alert('Please fill in all required fields.');
        }
    };

    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    };
});


// Controller for Phone Number Drilldown
app.controller('PhoneNumberDrilldownController', function ($scope, $http, $uibModalInstance, selectedDevice) {
    $scope.device = selectedDevice;
    $scope.phoneNumbers = [];

    // Load phone numbers associated with the device
    $scope.loadPhoneNumbers = function () {
        $http.get('/api/PhoneNumbers/SearchPhoneNumbersByDevicePage', { params: { deviceId: $scope.device.ID } })
            .then(function (response) {
                $scope.phoneNumbers = response.data;
            }, function (error) {
                console.error('Error loading phone numbers:', error);
                alert('An error occurred while loading phone numbers.');
            });
    };

    $scope.loadPhoneNumbers();

    // Function to edit a phone number
    $scope.editPhoneNumber = function (phoneNumber) {
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
        });
    };

    // Update the phone number
    $scope.updatePhoneNumber = function (updatedPhoneNumber) {
        $http.put('/api/PhoneNumbers/UpdatePhoneNumber', updatedPhoneNumber)
            .then(function (response) {
                $scope.loadPhoneNumbers(); // Refresh the list of phone numbers after editing
            }, function (error) {
                console.error('Error updating phone number:', error);
                alert('An error occurred while updating the phone number.');
            });
    };

    // Close the modal
    $scope.close = function () {
        $uibModalInstance.close();
    };
});
