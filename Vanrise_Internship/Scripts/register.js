
$(document).ready(function () {
    $('#registerForm').submit(function (e) {
        var password = $('#password').val();
        if (password.length < 8) {
            alert('Password must be at least 8 characters long.');
            e.preventDefault(); // Prevent form submission
        }
    });
});