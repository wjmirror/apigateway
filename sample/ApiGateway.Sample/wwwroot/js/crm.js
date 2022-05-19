// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function GetCustomerInClient() {

	var accountNumber = $("#txtAccountNumber").val();
	var url = "https://sscoweb12dev.spray.com/apigateway/crmapi/accounts?$select=accountid,accountnumber,address1_city,address1_country,address1_county,address1_line1,address1_line2,address1_line3,address1_name,address1_postalcode,address1_stateorprovince&$filter=accountnumber eq '" + accountNumber + "'";
    var settings = {
        "crossDomain": true,
        "url": url,
        "method": "GET",
        "xhrFields": {
            "withCredentials": true
        }
    }

    $.ajax(settings).done(function (response) {
		console.log(response);
		$("#txtResult").val(response);
    });

}

