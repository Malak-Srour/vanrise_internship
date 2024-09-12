var app = angular.module('clientReportApp', []);

app.controller('ReportController', function ($scope, $http) {
    $scope.clientReports = [];

    // Load client report data
    $http.get('/api/Reports/GetClientsPerType')
        .then(function (response) {
            $scope.clientReports = response.data;
        }, function (error) {
            console.error('Error loading client report:', error);
        });
});