
﻿//JS File for NewApplication
//Removed Jquery.ready Function

$(document).ready(function () {

	//Add scroll effect for terms and conditions
	$(document).ready(function () {
		$("#read_through_content").scroll(function () {
			var totalScrollHeight = $("#read_through_content")[0].scrollHeight;
			var scrollBarHeight = $("#read_through_content")[0].clientHeight;
			var scrollBarTopPosition = $("#read_through_content")[0].scrollTop;
			if (totalScrollHeight === scrollBarHeight + scrollBarTopPosition) {
				$("#applicant_read_the_content").val("true");
			}
		});

		//Not working for Chrome
		/*$("#terms").click(function () {
			if ($("#applicant_read_the_content").val() !== "true") {
				toastr.error('Please scroll through the disclosure text before clicking I Accept.', { positionClass: 'toast-top-center' });
				return false;
			}
		});*/

	});

	//On Load
    LoadUpModal();

    //Load up modal function
    function LoadUpModal() {
        $('#LoadUpModal').modal({ backdrop: 'static', keyboard: false });
    }
	
	//Load Up Settlement Modal
	function LoadUpSettlementModal() {
        $('#LoadUpSettlementModal').modal({ backdrop: 'static', keyboard: false });
    }
	
    //Load up signatory modal function
    function LoadUpSignatoryModal() {
        $('#LoadUpSignatoryModal').modal({ backdrop: 'static', keyboard: false });
    }

    //Load up representative modal function
    function LoadUpRepresentativeModal() {
        $('#LoadUpRepresentativeModal').modal({ backdrop: 'static', keyboard: false });
    }
    //Close modal
	$('#closemodal').on('click', function (e) {
		e.preventDefault();
		$('#LoadUpModal').modal('hide');
	});

    //btnCloseModal
	$('#btnCloseModal').on('click', function (e) {
		e.preventDefault();
		$('#LoadUpSignatoryModal').modal('hide');
	});

	$('#closemodalup').on('click', function (e) {
		e.preventDefault();
		$('#LoadUpModal').modal('hide');
	});

    //btnCloseRepModal btnEditRepresentatives LoadUpRepresentativeModal
	$('#btnCloseRepModal').on('click', function (e) {
		e.preventDefault();
		$('#LoadUpRepresentativeModal').modal('hide');
	});
	
	//btnCloseSetModal
	$('#btnCloseSetModal').on('click', function (e) {
		e.preventDefault();
		$('#LoadUpSettlementModal').modal('hide');
	});
    //Initialize Select2
    $('.select2').select2();

	//Get Company List
	$("#SelectedCompany").select2({
		placeholder: "Select Company",
		allowClear: true,
		ajax: {
			url: '/Client/GetCompanyList',
			data: function (params) {
				return {
					q: params.term // search term
				};
			},
			processResults: function (data) {
				return {
					results: data.items
				};
			},
			minimumInputLength: 2
		}
	});

	/* This is change event for your dropdownlist */
	$('#SelectedCompany').change(function () {
		/* Get the selected value of dropdownlist */
		var selectedID = $(this).val();

		/* Request the partial view with .get request. */
		$.get('/Client/_LoadCompanyDetails/' + selectedID, function (data) {
			/* data is the pure html returned from action method, load it to your page */
			$('#CompanyDetailsPartialView').html(data);
			$('#CompanyInfo').removeClass('d-none');
			/* little fade in effect */
			$('#CompanyDetailsPartialView').fadeIn('fast');
		});

		//applicant_terms_content
		$.get('../Content/documents/Merged GTCs and eMT Agreement/eMT-Agreement_and_Terms.html', function (data) {
			/* data is the pure html returned from action method, load it to your page */
			$('#applicant_esign_content').html(data);
			/* little fade in effect */
			$('#applicant_esign_content').fadeIn('fast');
		});
	});

    //Get Currency List
    $(".choose-currency").select2({
        placeholder: "Select Currency",
        allowClear: true,
        ajax: {
            url: '/Client/GetCurrencyList',
            data: function (params) {
                return {
                    q: params.term // search term
                };
            },
            processResults: function (data) {
                return {
                    results: data.items
                };
            },
            minimumInputLength: 2
        }
    });

    //Load modal
    function LoadUpSuccessModal() {
        $('#LoadSuccessModal').modal({ backdrop: 'static', keyboard: false });
    }

    //Form Validation
    var v = jQuery("#form").validate({
        rules: {
            BusinessEmailAddress: {
                email: true
            },
			CompanyName: { required: true },CompanyTownCity: { required: true },
            PostalCode: {
                number: true
            },
            "terms[]": {
                required: true,
                minlength: 1
            },
            SettlementAccount1: {
				number: true,
				minlength:13,
				maxlength: 13,
				accountStartingWith010: true
            },
            SettlementAccount2: {
				number: true,
				minlength: 13,
				maxlength: 13,
				accountStartingWith010: true
            },
            SettlementAccount3: {
				number: true,
				minlength: 13,
				maxlength: 13,
				accountStartingWith010: true
			},
			SettlementAccount4: {
				number: true,
				minlength: 13,
				maxlength: 13,
				accountStartingWith010: true
			},
            SettlementAccount5: {
				number: true,
				minlength: 13,
				maxlength: 13,
				accountStartingWith010: true
            },
            SignatorySurname2: {
                required: true
            },
            SignatoryOtherNames2: {
                required: true
            },
            SignatoryDesignation2: {
                required: true
            },
            SignatoryEmail2: {
                required: true,
				email: true,
				notEqualTo: "#SignatoryEmail1"
            },
            SignatorySurname3: {
                required: true
            },
            SignatoryOtherNames3: {
                required: true
            },
            SignatoryDesignation3: {
                required: true
            },
            SignatoryEmail3: {
                required: true,
				email: true,
				notEqualTo: "#SignatoryEmail1"
            },
            SignatorySurname4: {
                required: true
            },
            SignatoryOtherNames4: {
                required: true
            },
            SignatoryDesignation4: {
                required: true
            },
            SignatoryEmail4: {
                required: true,
				email: true,
				notEqualTo: "#SignatoryEmail1"
            },
            SignatorySurname5: {
                required: true
            },
            SignatoryOtherNames5: {
                required: true
            },
            SignatoryDesignation5: {
                required: true
            },
            SignatoryEmail5: {
                required: true,
				email: true,
				notEqualTo: "#SignatoryEmail1"
            },
            UserZipCode1: { number:true }, UserZipCode2: {  number: true  },  UserZipCode3: {  number: true }, UserZipCode4: {  number: true }, UserZipCode5: { number: true },
			TransactionLimit1: { number: true }, TransactionLimit2: { number: true }, TransactionLimit3: { number: true }, TransactionLimit4: { number: true }, TransactionLimit5: { number: true },
			TransactionLimitQ1: { required: true }, TransactionLimitQ2: { required: true }, TransactionLimitQ3: { required: true }, TransactionLimitQ4: { required: true }, TransactionLimitQ5: { required: true },
			EMarketSignUp1: { required: true }, EMarketSignUp2: { required: true }, EMarketSignUp3: { required: true }, EMarketSignUp4: { required: true }, EMarketSignUp5: { required: true },
			inputFile: {
				required: true,
				accept: "jpg,png,jpeg"
			}
        },
        messages: {
            inputFile: {
                required: "Please upload your signature",
                accept: "Only images of type jpg/png/jpeg are allowed"
            },
            "terms[]": "Please read carefully and accept the terms and conditions"
        }   
    });

	$('#SignatoryEmail3').on('keyup focusout', function (e) {
		var email2 = $('#SignatoryEmail2').val();
		var email3 = $('#SignatoryEmail3').val();
		if (email2 === email3) {
			$('#SignatoryEmail3').removeClass('valid');
			$('#SignatoryEmail3').addClass('error');
		}
	});

	$('#closemodal').on('click', function () {
		$('#LoadUpModal').modal('hide');
	});

    //Initialize Date picker
    $('.datepicker').datepicker({
        format: "yyyy/mm/dd",
        autoclose: true
    }).on('changeDate', function (e) {
        $(this).datepicker('hide');
    });

    var options = {
        mode: 'wizard',
        autoButtonsNextClass: 'btn btn-primary float-right',
        autoButtonsPrevClass: 'btn btn-light',
        stepNumberClass: 'badge badge-pill badge-primary mr-1',
        onSubmit: function () {
            alert('Form submitted!');
            return true;
        }
    }

    $(function () {
        $("#form").accWizard(options);
    });

	//Accepted Terms
	var acceptedTerms = $('#AcceptedTerms').val();
	if (acceptedTerms === "True") {
		$("#terms").prop('checked', true);
		$('#ConfirmTerms').html("Accepted");
	}
	else {
		$("#terms").prop('checked', false);
	}

    //Show Account on settlement accounts
    $("#HaveSettlementAccount").change(function () {
        if (this.value === "Yes") {
            $('#AccountDiv1').removeClass('d_none');
            $('#SettlementAccount1').addClass('required');
            $('#SettlementAccount2').addClass('required');
            $('#SelectCurrency1').addClass('required');
            $('#SelectCurrency2').addClass('required');
            $('#AccountDiv2').removeClass('d_none');
            $('#disclaimer').removeClass('d_none');
            $('#btnAddAccount3').removeClass('d_none');
            $('#btn-save-settlement').removeClass('d_none');
        }
        else {
            $('#AccountDiv1').addClass('d_none');
            $('#AccountDiv2').addClass('d_none');
            $('#AccountDiv3').addClass('d_none');
            $('#AccountDiv4').addClass('d_none');
            $('#AccountDiv5').addClass('d_none');
            $('#disclaimer').addClass('d_none');
            $('#SettlementAccount1').removeClass('required');
            $('#SettlementAccount2').removeClass('required');
            $('#SelectCurrency1').removeClass('required');
            $('#SelectCurrency2').removeClass('required');
            $('#btn-save-settlement').addClass('d_none');
        }
    });

    // Show div 3 on add account
    $('#btnAddAccount3').on('click', function () {
        $('#AccountDiv3').removeClass('d_none');
        $('#btnAddAccount3').addClass('d_none');
		$('#btn-save-settlement').removeClass('d_none');
		$('#savedsettlement').addClass('d_none');
    });
	
	//Remove Settlement Account 3
	$('#btnRemoveAccount3').on('click', function (e) {
		e.preventDefault();
		//Post to check & remove if sett account was saved
		var account = $('#SettlementAccount3').val();
		var companyid = $('#CompanyID').val();
		if (jQuery.trim(account).length > 0) {
			$.ajax({
				url: '/Client/RemoveSettlementAccount',
				type: 'POST',
				data: '{account: "' + account + '", companyId: "' + companyid + '" }',
				contentType: "application/json; charset=utf-8",
				dataType: "html",
				success: function () {
					$('#AccountDiv3').addClass('d_none');
					$('#btnAddAccount3').removeClass('d_none');
					$('#SettlementAccount3').val("");
					$('#InputCurrencyType3 input').removeClass('help-inline-error');
					$('#InputCurrencyType3').find('span').remove();
					$('#SettlementAccount3').val("");
					$('#InputCurrencyType3').val("");
				},
				error: function (xhr, textStatus, errorThrown) {
					if (textStatus === 'error') {
						toastr.error('Submission error!. Code: ' + xhr.status + ', Details: ' + errorThrown);
					}
				}
			});
		}
		else {
			$('#AccountDiv3').addClass('d_none');
			$('#btnAddAccount3').removeClass('d_none');
			$('#SettlementAccount3').val("");
			$('#InputCurrencyType3 input').removeClass('help-inline-error');
			$('#InputCurrencyType3').find('span').remove();
			$('#SettlementAccount3').val("");
		}
	});

    // Show div 4 on add account
    $('#btnAddAccount4').on('click', function () {
        $('#AccountDiv4').removeClass('d_none');
        $('#btnAddAccount4').addClass('d_none');
        $('#btnRemoveAccount3').addClass('d_none');
        $('#btnRemoveAccount4').removeClass('d_none');
    });
	
	//Remove Settlement Account 4
	$('#btnRemoveAccount4').on('click', function (e) {
		e.preventDefault();
		//Post to check & remove if sett account was saved
		var account = $('#SettlementAccount4').val();
		var companyid = $('#CompanyID').val();
		if (jQuery.trim(account).length > 0) {
			$.ajax({
				url: '/Client/RemoveSettlementAccount',
				type: 'POST',
				data: '{account: "' + account + '", companyId: "' + companyid + '" }',
				contentType: "application/json; charset=utf-8",
				dataType: "html",
				success: function () {
					$('#AccountDiv4').addClass('d_none');
					$('#btnAddAccount4').removeClass('d_none');
					$('#btnRemoveAccount3').removeClass('d_none');
					$('#InputCurrencyType4 input').removeClass('help-inline-error');
					$('#InputCurrencyType4').find('span').remove();
					$('#SettlementAccount4').val("");
					$('#InputCurrencyType4').val("");
				},
				error: function (xhr, textStatus, errorThrown) {
					if (textStatus === 'error') {
						toastr.error('Error!. Code: ' + xhr.status + ', Details: ' + errorThrown);
					}
				}
			});
		}
		else {
			$('#AccountDiv4').addClass('d_none');
			$('#btnAddAccount4').removeClass('d_none');
			$('#btnRemoveAccount3').removeClass('d_none');
			$('#InputCurrencyType4 input').removeClass('help-inline-error');
			$('#InputCurrencyType4').find('span').remove();
			$('#SettlementAccount4').val("");
		}
	});
    
    // Show div 5 on add account
    $('#btnAddAccount5').on('click', function () {
        $('#AccountDiv5').removeClass('d_none');
        $('#btnAddAccount5').addClass('d_none');
        $('#btnRemoveAccount4').addClass('d_none');
        $('#btnRemoveAccount5').removeClass('d_none');
    });
	
	//Remove Settlement Account 5
	$('#btnRemoveAccount5').on('click', function (e) {
		e.preventDefault();
		//Post to check & remove if sett account was saved
		var account = $('#SettlementAccount5').val();
		var companyid = $('#CompanyID').val();
		if (jQuery.trim(account).length > 0) {
			$.ajax({
				url: '/Client/RemoveSettlementAccount',
				type: 'POST',
				data: '{account: "' + account + '", companyId: "' + companyid + '" }',
				contentType: "application/json; charset=utf-8",
				dataType: "html",
				success: function () {
					$('#AccountDiv5').addClass('d_none');
					$('#btnAddAccount5').removeClass('d_none');
					$('#btnRemoveAccount4').removeClass('d_none');
					$('#InputCurrencyType5 input').removeClass('help-inline-error');
					$('#InputCurrencyType5').find('span').remove();
					$('#SettlementAccount5').val("");
					$('#InputCurrencyType5').val("");
				},
				error: function (xhr, textStatus, errorThrown) {
					if (textStatus === 'error') {
						toastr.error('Submission error!. Code: ' + xhr.status + ', Details: ' + errorThrown);
					}
				}
			});
		}
		else {
			$('#AccountDiv5').addClass('d_none');
			$('#btnAddAccount5').removeClass('d_none');
			$('#btnRemoveAccount4').removeClass('d_none');
			$('#InputCurrencyType5 input').removeClass('help-inline-error');
			$('#InputCurrencyType5').find('span').remove();
			$('#SettlementAccount5').val("");
		}
	});
     
    //EMarketSignUp5 disclaimerUser5
    $("#EMarketSignUp1").change(function () {
        //var chosenvalue = this.value;
        if (this.value === "true") {
			$('#disclaimerUser1').removeClass('d_none');
			$('#disclaimerNotUser1').addClass('d_none');
        } else {
            $('#disclaimerUser1').addClass('d_none');
			$('#disclaimerNotUser1').removeClass('d_none');
        }
    });
    $("#EMarketSignUp2").change(function () {
        //var chosenvalue = this.value;
        if (this.value === "true") {
			$('#disclaimerUser2').removeClass('d_none');
			$('#disclaimerNotUser2').addClass('d_none');
        } else {
			$('#disclaimerUser2').addClass('d_none');
			$('#disclaimerNotUser2').removeClass('d_none');
        }
    });
    $("#EMarketSignUp3").change(function () {
        //var chosenvalue = this.value;
        if (this.value === "true") {
			$('#disclaimerUser3').removeClass('d_none');
			$('#disclaimerNotUser3').addClass('d_none');
        } else {
			$('#disclaimerUser3').addClass('d_none');
			$('#disclaimerNotUser3').removeClass('d_none');
        }
    });
    $("#EMarketSignUp4").change(function () {
        //var chosenvalue = this.value;
        if (this.value === "true") {
			$('#disclaimerUser4').removeClass('d_none');
			$('#disclaimerNotUser4').addClass('d_none');
        } else {
			$('#disclaimerUser4').addClass('d_none');
			$('#disclaimerNotUser4').removeClass('d_none');
        }
    });
    $("#EMarketSignUp5").change(function () {
        //var chosenvalue = this.value;
        if (this.value === "true") {
			$('#disclaimerUser5').removeClass('d_none');
			$('#disclaimerNotUser5').addClass('d_none');
        } else {
			$('#disclaimerUser5').addClass('d_none');
			$('#disclaimerNotUser5').removeClass('d_none');
        }
    });
    //Show Account on settlement accounts
    $("#TransactionLimitQ1").change(function () {
        //var chosenvalue = this.value;
        if (this.value === "Yes") {
            $('.TransactionLimit1').removeClass('d_none');
            $('#TransactionLimit1').addClass('required');
        } else {
            $('.TransactionLimit1').addClass('d_none');
            $('#TransactionLimit1').removeClass('required');
            $('#TransactionLimit1').removeClass('has-error');
            $('#TransactionLimit1').val('No Limit');
        }
    });
    //Show Account on settlement accounts
    $("#TransactionLimitQ2").change(function () {
        //var chosenvalue = this.value;
        if (this.value === "Yes") {
            $('.TransactionLimit2').removeClass('d_none');
            $('#TransactionLimit2').addClass('required');
        } else {
            $('.TransactionLimit2').addClass('d_none');
            $('#TransactionLimit2').removeClass('required');
            $('#TransactionLimit2').removeClass('has-error');
            $('#TransactionLimit2').val('No Limit');
        }
    });
    //Show Account on settlement accounts
    $("#TransactionLimitQ3").change(function () {
        //var chosenvalue = this.value;
        if (this.value === "Yes") {
            $('.TransactionLimit3').removeClass('d_none');
            $('#TransactionLimit3').addClass('required');
        } else {
            $('.TransactionLimit3').addClass('d_none');
            $('#TransactionLimit3').removeClass('required');
            $('#TransactionLimit3').removeClass('has-error');
            $('#TransactionLimit3').val('No Limit');
        }
    });
    //Show Account on settlement accounts
    $("#TransactionLimitQ4").change(function () {
        //var chosenvalue = this.value;
        if (this.value === "Yes") {
            $('.TransactionLimit4').removeClass('d_none');
            $('#TransactionLimit4').addClass('required');
        } else {
            $('.TransactionLimit4').addClass('d_none');
            $('#TransactionLimit4').removeClass('required');
            $('#TransactionLimit4').removeClass('has-error');
            $('#TransactionLimit4').val('No Limit');
        }
    });
    //Show Account on settlement accounts
    $("#TransactionLimitQ5").change(function () {
        //var chosenvalue = this.value;
        if (this.value === "Yes") {
            $('.TransactionLimit5').removeClass('d_none');
            $('#TransactionLimit5').addClass('required');
        } else {
            $('.TransactionLimit5').addClass('d_none');
            $('#TransactionLimit5').removeClass('required');
            $('#TransactionLimit5').removeClass('has-error');
            $('#TransactionLimit5').val('No Limit');
        }
	});

    //Manipulate settlement accounts on other select
	$('#SelectCurrency1').on('change', function () {
		var otherselected = this.value;
		if (otherselected === 6) {
			$('#currencytypediv1').removeClass('d_none');
		} else {
			$('#currencytypediv1').addClass('d_none');
			$('#InputCurrencyType1 input').removeClass('help-inline-error');
			$('#InputCurrencyType1').find('span').remove();
			$('#InputCurrencyType1 input').val('');
		}
	});

    //Manipulate settlement accounts on other select
	$('#SelectCurrency2').on('change', function () {
		var otherselected = this.value;
		if (otherselected === 6) {
			$('#currencytypediv2').removeClass('d_none');
		} else {
			$('#currencytypediv2').addClass('d_none');
			$('#InputCurrencyType2 input').removeClass('help-inline-error');
			$('#InputCurrencyType2').find('span').remove();
			$('#InputCurrencyType2 input').val('');
		}
	});

    //Manipulate user Panels Div
    $('#btnAddUser2').on('click', function () {
        $('#UserDiv2').removeClass('d_none');
        $('#btnAddUser2').addClass('d_none');
        $('#UserSurname2').addClass('required');
        $('#UserOthernames2').addClass('required');
        $('#UserEmail2').addClass('required');
        $('#UserMobileNumber2').addClass('required');
    });
	
	//Remove User 2
	$('#btnRemoveUser2').on('click', function (e) {
		e.preventDefault();
		//Post to check & remove if sett account was saved
		var email = $('#UserEmail2').val();
		if (jQuery.trim(email).length > 0) {
			$.ajax({
				url: '/Client/RemoveRepresentative',
				type: 'POST',
				data: '{email: "' + email + '" }',
				contentType: "application/json; charset=utf-8",
				dataType: "html",
				success: function () {
					$('#UserDiv2').addClass('d_none');
					$('#btnAddUser2').removeClass('d_none');
					$('#UserDiv2 input').removeClass('help-inline-error');
					$('#UserDiv2 input').removeClass('input-error');
					$('#UserDiv2 input').val('');
					$('#UserSurname2').removeClass('required');
					$('#UserOthernames2').removeClass('required');
					$('#UserEmail2').removeClass('required');
					$('#UserMobileNumber2').removeClass('required');
				},
				error: function (xhr, textStatus, errorThrown) {
					if (textStatus === 'error') {
						toastr.error('Submission error!. Code: ' + xhr.status + ', Details: ' + errorThrown);
					}
				}
			});
		}
		else {
			$('#UserDiv2').addClass('d_none');
			$('#btnAddUser2').removeClass('d_none');
			$('#UserDiv2 input').removeClass('help-inline-error');
			$('#UserDiv2 input').removeClass('input-error');
			$('#UserDiv2 input').val('');
			$('#UserSurname2').removeClass('required');
			$('#UserOthernames2').removeClass('required');
			$('#UserEmail2').removeClass('required');
			$('#UserMobileNumber2').removeClass('required');
		}
	});
	
    $('#btnAddUser3').on('click', function () {
        $('#UserDiv3').removeClass('d_none');
        $('#btnAddUser3').addClass('d_none');
        $('#btnRemoveUser2').addClass('d_none');
        $('#UserSurname3').addClass('required');
        $('#UserOthernames3').addClass('required');
        $('#UserEmail3').addClass('required');
        $('#UserMobileNumber3').addClass('required');
    });
	
	//Remove User 3
	$('#btnRemoveUser3').on('click', function (e) {
		e.preventDefault();
		//Post to check & remove if sett account was saved
		var email = $('#UserEmail3').val();
		if (jQuery.trim(email).length > 0) {
			$.ajax({
				url: '/Client/RemoveRepresentative',
				type: 'POST',
				data: '{email: "' + email + '" }',
				contentType: "application/json; charset=utf-8",
				dataType: "html",
				success: function () {
					$('#UserDiv3').addClass('d_none');
					$('#btnAddUser3').removeClass('d_none');
					$('#btnRemoveUser2').removeClass('d_none');
					$('#UserDiv3').find('span').remove();
					$('#UserDiv3 input').removeClass('help-inline-error');
					$('#UserDiv3 input').val('');
					$('#UserSurname3').removeClass('required');
					$('#UserOthernames3').removeClass('required');
					$('#UserEmail3').removeClass('required');
					$('#UserMobileNumber3').removeClass('required');
				},
				error: function (xhr, textStatus, errorThrown) {
					if (textStatus === 'error') {
						toastr.error('Submission error!. Code: ' + xhr.status + ', Details: ' + errorThrown);
					}
				}
			});
		}
		else {
			$('#UserDiv3').addClass('d_none');
			$('#btnAddUser3').removeClass('d_none');
			$('#btnRemoveUser2').removeClass('d_none');
			$('#UserDiv3').find('span').remove();
			$('#UserDiv3 input').removeClass('help-inline-error');
			$('#UserDiv3 input').val('');
			$('#UserSurname3').removeClass('required');
			$('#UserOthernames3').removeClass('required');
			$('#UserEmail3').removeClass('required');
			$('#UserMobileNumber3').removeClass('required');
		}
	});
     
    $('#btnAddUser4').on('click', function () {
        $('#UserDiv4').removeClass('d_none');
        $('#btnAddUser4').addClass('d_none');
        $('#btnRemoveUser3').addClass('d_none');
        $('#UserSurname4').addClass('required');
        $('#UserOthernames4').addClass('required');
        $('#UserEmail4').addClass('required');
        $('#UserMobileNumber4').addClass('required');
    });
	//Remove User 4
	$('#btnRemoveUser4').on('click', function (e) {
		e.preventDefault();
		//Post to check & remove if sett account was saved
		var email = $('#UserEmail4').val();
		if (jQuery.trim(email).length > 0) {
			$.ajax({
				url: '/Client/RemoveRepresentative',
				type: 'POST',
				data: '{email: "' + email + '" }',
				contentType: "application/json; charset=utf-8",
				dataType: "html",
				success: function () {
					$('#UserDiv4').addClass('d_none');
					$('#btnAddUser4').removeClass('d_none');
					$('#btnRemoveUser3').removeClass('d_none');
					$('#UserDiv4').find('span').remove();
					$('#UserDiv4 input').removeClass('help-inline-error');
					$('#UserDiv4 input').val('');
					$('#UserSurname4').removeClass('required');
					$('#UserOthernames4').removeClass('required');
					$('#UserEmail4').removeClass('required');
					$('#UserMobileNumber4').removeClass('required');
				},
				error: function (xhr, textStatus, errorThrown) {
					if (textStatus === 'error') {
						toastr.error('Submission error!. Code: ' + xhr.status + ', Details: ' + errorThrown);
					}
				}
			});
		}
		else {
			$('#UserDiv4').addClass('d_none');
			$('#btnAddUser4').removeClass('d_none');
			$('#btnRemoveUser3').removeClass('d_none');
			$('#UserDiv4').find('span').remove();
			$('#UserDiv4 input').removeClass('help-inline-error');
			$('#UserDiv4 input').val('');
			$('#UserSurname4').removeClass('required');
			$('#UserOthernames4').removeClass('required');
			$('#UserEmail4').removeClass('required');
			$('#UserMobileNumber4').removeClass('required');
		}
	});
     
    $('#btnAddUser5').on('click', function () {
        $('#UserDiv5').removeClass('d_none');
        $('#btnAddUser5').addClass('d_none');
        $('#btnRemoveUser4').addClass('d_none');
        $('#UserSurname5').addClass('required');
        $('#UserOthernames5').addClass('required');
        $('#UserEmail5').addClass('required');
        $('#UserMobileNumber5').addClass('required');
    });
	
	//Remove User 4
	$('#btnRemoveUser5').on('click', function (e) {
		e.preventDefault();
		//Post to check & remove if sett account was saved
		var email = $('#UserEmail5').val();
		if (jQuery.trim(email).length > 0) {
			$.ajax({
				url: '/Client/RemoveRepresentative',
				type: 'POST',
				data: '{email: "' + email + '" }',
				contentType: "application/json; charset=utf-8",
				dataType: "html",
				success: function () {
					$('#UserDiv5').addClass('d_none');
					$('#btnAddUser5').removeClass('d_none');
					$('#UserDiv5').find('span').remove();
					$('#UserDiv5 input').removeClass('help-inline-error');
					$('#btnRemoveUser4').removeClass('d_none');
					$('#UserDiv5 input').val('');
					$('#UserSurname5').removeClass('required');
					$('#UserOthernames5').removeClass('required');
					$('#UserEmail5').removeClass('required');
					$('#UserMobileNumber5').removeClass('required');
				},
				error: function (xhr, textStatus, errorThrown) {
					if (textStatus === 'error') {
						toastr.error('Submission error!. Code: ' + xhr.status + ', Details: ' + errorThrown);
					}
				}
			});
		}
		else {
			$('#UserDiv5').addClass('d_none');
			$('#btnAddUser5').removeClass('d_none');
			$('#UserDiv5').find('span').remove();
			$('#UserDiv5 input').removeClass('help-inline-error');
			$('#btnRemoveUser4').removeClass('d_none');
			$('#UserDiv5 input').val('');
			$('#UserSurname5').removeClass('required');
			$('#UserOthernames5').removeClass('required');
			$('#UserEmail5').removeClass('required');
			$('#UserMobileNumber5').removeClass('required');
		}
	});

    $(".imgAdd").click(function () {
        $(this).closest(".row").find('.imgAdd').before('<div class="col-sm-2 imgUp"><div class="imagePreview"></div><label class="btn btn-primary">Upload<input type="file" class="uploadFile img" value="Upload Photo" style="width:0px;height:0px;overflow:hidden;"></label><i class="fa fa-times del"></i></div>');
    });

    $(document).on("click", "i.del", function () {
        $(this).parent().remove();
	});

    $(function () {
        $(document).on("change", ".uploadFile", function () {
            var uploadFile = $(this);
            var files = !!this.files ? this.files : [];
            if (!files.length || !window.FileReader) return; // no file selected, or no FileReader support

            if (/^image/.test(files[0].type)) { // only image file
                var reader = new FileReader(); // instance of the FileReader
                reader.readAsDataURL(files[0]); // read the local file

                reader.onloadend = function () { // set image data as background of div
                    //alert(uploadFile.closest(".upimage").find('.imagePreview').length);
                    uploadFile.closest(".imgUp").find('.imagePreview').css("background-image", "url(" + this.result + ")");
                }
            }

        });
    });

    //Manipulate Signatories Panel Divs
    $('#btnAddSignatory2').on('click', function () {
        $('#SignatoryDiv2').removeClass('d_none');
        $('#btnAddSignatory2').addClass('d_none');
		$('#savedsignatories').addClass('d_none');
		$('#btn-save-signatories').removeClass('d_none');
    });
	
	$('#btnRemoveSignatory2').on('click', function (e) {
		e.preventDefault();
		//Post to check & remove if account was saved
		var email = $('#SignatoryEmail2').val();
		if (jQuery.trim(email).length > 0) {
			$.ajax({
				url: '/Client/RemoveSignatory',
				type: 'POST',
				data: '{email: "' + email + '" }',
				contentType: "application/json; charset=utf-8",
				dataType: "html",
				success: function () {
					$('#SignatoryDiv2').addClass('d_none');
					$('#btnAddSignatory2').removeClass('d_none');
					$('#SignatoryDiv2 input').removeClass('help-inline-error');
					$('#SignatoryDiv2').find('span').remove();
					$('#SignatoryDiv2 input').val('');
				},
				error: function (xhr, textStatus, errorThrown) {
					if (textStatus === 'error') {
						toastr.error('Submission error!. Code: ' + xhr.status + ', Details: ' + errorThrown);
					}
				}
			});
		}
		else {
			$('#SignatoryDiv2').addClass('d_none');
			$('#btnAddSignatory2').removeClass('d_none');
			$('#SignatoryDiv2 input').removeClass('help-inline-error');
			$('#SignatoryDiv2').find('span').remove();
			$('#SignatoryDiv2 input').val('');
		}
	});

    $('#btnAddSignatory3').on('click', function () {
        $('#SignatoryDiv3').removeClass('d_none');
        $('#btnAddSignatory3').addClass('d_none');
		$('#btnRemoveSignatory2').addClass('d_none');
		$('#savedsignatories').addClass('d_none');
		$('#btn-save-signatories').removeClass('d_none');
	});

	$('#btnRemoveSignatory3').on('click', function (e) {
		e.preventDefault();
		//Post to check & remove if account was saved
		var email = $('#btnRemoveSignatory3').val();
		if (jQuery.trim(email).length > 0) {
			$.ajax({
				url: '/Client/RemoveSignatory',
				type: 'POST',
				data: '{email: "' + email + '" }',
				contentType: "application/json; charset=utf-8",
				dataType: "html",
				success: function () {
					$('#SignatoryDiv3').addClass('d_none');
					$('#btnAddSignatory3').removeClass('d_none');
					$('#btnRemoveSignatory2').removeClass('d_none');
					$('#SignatoryDiv3 input').removeClass('help-inline-error');
					$('#SignatoryDiv3').find('span').remove();
					$('#SignatoryDiv3 input').val('');
				},
				error: function (xhr, textStatus, errorThrown) {
					if (textStatus === 'error') {
						toastr.error('Submission error!. Code: ' + xhr.status + ', Details: ' + errorThrown);
					}
				}
			});
		}
		else {
			$('#SignatoryDiv3').addClass('d_none');
			$('#btnAddSignatory3').removeClass('d_none');
			$('#btnRemoveSignatory2').removeClass('d_none');
			$('#SignatoryDiv3 input').removeClass('help-inline-error');
			$('#SignatoryDiv3').find('span').remove();
			$('#SignatoryDiv3 input').val('');
		}
	});

    $('#btnAddSignatory4').on('click', function () {
        $('#SignatoryDiv4').removeClass('d_none');
        $('#btnAddSignatory4').addClass('d_none');
		$('#btnRemoveSignatory3').addClass('d_none');
		$('#savedsignatories').addClass('d_none');
		$('#btn-save-signatories').removeClass('d_none');
	});

	$('#btnRemoveSignatory4').on('click', function (e) {
		e.preventDefault();
		//Post to check & remove if account was saved
		var email = $('#btnRemoveSignatory4').val();
		if (jQuery.trim(email).length > 0) {
			$.ajax({
				url: '/Client/RemoveSignatory',
				type: 'POST',
				data: '{email: "' + email + '" }',
				contentType: "application/json; charset=utf-8",
				dataType: "html",
				success: function () {
					$('#SignatoryDiv4').addClass('d_none');
					$('#btnAddSignatory4').removeClass('d_none');
					$('#btnRemoveSignatory3').removeClass('d_none');
					$('#SignatoryDiv4 input').removeClass('help-inline-error');
					$('#SignatoryDiv4').find('span').remove();
					$('#SignatoryDiv4 input').val('');
				},
				error: function (xhr, textStatus, errorThrown) {
					if (textStatus === 'error') {
						toastr.error('Submission error!. Code: ' + xhr.status + ', Details: ' + errorThrown);
					}
				}
			});
		}
		else {
			$('#SignatoryDiv4').addClass('d_none');
			$('#btnAddSignatory4').removeClass('d_none');
			$('#btnRemoveSignatory3').removeClass('d_none');
			$('#SignatoryDiv4 input').removeClass('help-inline-error');
			$('#SignatoryDiv4').find('span').remove();
			$('#SignatoryDiv4 input').val('');
		}
	});
   
    $('#btnAddSignatory5').on('click', function () {
        $('#SignatoryDiv5').removeClass('d_none');
        $('#btnAddSignatory5').addClass('d_none');
		$('#btnRemoveSignatory4').addClass('d_none');
		$('#savedsignatories').addClass('d_none');
		$('#btn-save-signatories').removeClass('d_none');
	});

	$('#btnRemoveSignatory5').on('click', function (e) {
		e.preventDefault();
		//Post to check & remove if account was saved
		var email = $('#btnRemoveSignatory5').val();
		if (jQuery.trim(email).length > 0) {
			$.ajax({
				url: '/Client/RemoveSignatory',
				type: 'POST',
				data: '{email: "' + email + '" }',
				contentType: "application/json; charset=utf-8",
				dataType: "html",
				success: function () {
					$('#SignatoryDiv5').addClass('d_none');
					$('#btnAddSignatory5').removeClass('d_none');
					$('#btnRemoveSignatory4').removeClass('d_none');
					$('#SignatoryDiv5 input').removeClass('help-inline-error');
					$('#SignatoryDiv5').find('span').remove();
					$('#SignatoryDiv5 input').val('');
				},
				error: function (xhr, textStatus, errorThrown) {
					if (textStatus === 'error') {
						toastr.error('Submission error!. Code: ' + xhr.status + ', Details: ' + errorThrown);
					}
				}
			});
		}
		else {
			$('#SignatoryDiv5').addClass('d_none');
			$('#btnAddSignatory5').removeClass('d_none');
			$('#btnRemoveSignatory4').removeClass('d_none');
			$('#SignatoryDiv5 input').removeClass('help-inline-error');
			$('#SignatoryDiv5').find('span').remove();
			$('#SignatoryDiv5 input').val('');
		}
	});

    //Save company details
    $('#save').on('click', function (e) {
        e.preventDefault();
        if ($('#form').valid()) {
			$("#save").addClass("d_none");
			$("#saving").removeClass("d_none");
            var formData = new FormData($("#form")[0]);
            $.ajax({
                url: '/Client/SaveUserInfo',
                type: 'POST',
                data: formData,
                async: true,
                success: function () {
                    window.setTimeout(close, 500);
                    window.setTimeout(enablesave, 3000);
                    function close() {
						$("#saving").addClass("d_none");
						$("#saved").removeClass("d_none");
                        toastr.success('Company details saved', { positionClass: 'toast-top-center' });
                    }
					function enablesave() {
						$("#saved").addClass("d_none");
						$("#save").removeClass("d_none");
                    }
				},
				error: function (xhr, textStatus, errorThrown) {
					if (textStatus === 'error') {
						$("#saved").addClass("d_none");
						$("#save").removeClass("d_none");
						toastr.error('Submission error!. Code: ' + xhr.status + ', Details: ' + errorThrown);
					}
				},
                cache: false,
                contentType: false,
                processData: false
            });
        }
        else {
            toastr.error('Please correct the errors on form inputs to save');
        }
    });

    var settlements = $('#SettlementAccountsCount').val();
    if (settlements > 0) {
        $('#btn-save-settlement').addClass('d_none');
        $('#btn-edit-settlement').removeClass('d_none');
        $('#HaveSettlementsDiv').removeClass('d_none');
        $('#SettlementAccountDiv').addClass('d_none');
        $('#SavedSettlements').removeClass('d_none');
        $('#HaveSettlementAccount').attr("disabled", true);
		$("#HaveSettlementAccount").val("Yes");
		//Clear Settlement Instructions textboxes
		$("#SettlementAccount1").val("");
		$("#SettlementAccount2").val("");
		$("#SettlementAccount3").val("");
		$("#SettlementAccount4").val("");
		$("#SettlementAccount5").val("");
		//Clear currency types
		$("#InputCurrencyType1").val("");
		$("#InputCurrencyType2").val("");
		$("#InputCurrencyType3").val("");
		$("#InputCurrencyType4").val("");
		$("#InputCurrencyType5").val("");
    }
    else
    {
        $('#btn-edit-settlement').addClass('d_none');
		$('#btn-save-settlement').addClass('d_none');
		$('#HaveSettlementsDiv').addClass('d_none');
		$('#SavedSettlements').addClass('d_none');
		//Clear Settlement Instructions textboxes
		$("#SettlementAccount1").val("");
		$("#SettlementAccount2").val("");
		$("#SettlementAccount3").val("");
		$("#SettlementAccount4").val("");
		$("#SettlementAccount5").val("");
		//Clear currency types
		$("#InputCurrencyType1").val("");
		$("#InputCurrencyType2").val("");
		$("#InputCurrencyType3").val("");
		$("#InputCurrencyType4").val("");
		$("#InputCurrencyType5").val("");
    }
	
	//btn-edit-settlements load modal
    $('#btn-edit-settlement').on('click', function (e) {
        e.preventDefault();
        LoadUpSettlementModal();
    });
	
    //Select Edit settlements
    $('#btnEditSettlements').on('click', function (e) {
        e.preventDefault();
        $('#HaveSettlementsDiv').addClass('d_none');
        $('#HaveSettlementAccount').val('');
		$('#SettlementAccountDiv').removeClass('d_none');
        $('#HaveSettlementAccount').attr("disabled", false);
		$('#SettlementAccountsCount').val("0");
		var formData = new FormData($("#form")[0]);
        $.ajax({
            url: '/Client/ClearSettlementAccounts',
			type: 'POST',
			data: formData,
            async: true,
            success: function (XmlHttpRequest) {
                window.setTimeout(saved, 500);
                function saved() {
					if (XmlHttpRequest === 'success') {
						$('#AccountDiv1').removeClass('d_none');
						$('#AccountDiv2').removeClass('d_none');
						$('#btn-save-settlement').removeClass('d_none');
						$('#btn-edit-settlement').addClass('d_none');
						$('#LoadUpSettlementModal').modal('hide');
					}
					else {
						//toastr.error(XmlHttpRequest, { positionClass: 'toast-top-center' });
						$('#AccountDiv1').removeClass('d_none');
						$('#AccountDiv2').removeClass('d_none');
						$('#btn-save-settlement').removeClass('d_none');
						$('#btn-edit-settlement').addClass('d_none');
						$('#LoadUpSettlementModal').modal('hide');
					}
                }
			},
			error: function (xhr, textStatus, errorThrown) {
				if (textStatus === 'error') {
					toastr.error('Submission error!. Code: ' + xhr.status + ', Details: ' + errorThrown);
				}
			},
            cache: false,
            contentType: false,
            processData: false
        });
    });

    //Save settlement details
    $('#btn-save-settlement').on('click', function (e) {
        e.preventDefault();
        if ($('#form').valid()) {
			$("#btn-save-settlement").addClass("d_none");
			$("#savingsettlement").removeClass("d_none");
            var formData = new FormData($("#form")[0]);
            $.ajax({
                url: '/Client/SaveSettlementAccounts',
                type: 'POST',
                data: formData,
                async: true,
                success: function (XmlHttpRequest) {
                    window.setTimeout(close, 500);
					window.setTimeout(enablesave, 3000);
                    function close() {
						if (XmlHttpRequest === 'success') {
							$("#savingsettlement").addClass("d_none");
							$("#savedsettlement").removeClass("d_none");
							toastr.success('Settlement instructions saved', { positionClass: 'toast-top-center' });
						}
						else {
							$("#savingsettlement").addClass("d_none");
							$("#savedsettlement").removeClass("d_none");
							toastr.error('Error Saving your settlement instructions', { positionClass: 'toast-top-center' });
						}
                    }
					function enablesave() {
						$("#savedsettlement").addClass("d_none");
						$("#btn-save-settlement").removeClass("d_none");
					}
				},
				error: function (xhr, textStatus, errorThrown) {
					if (textStatus === 'error') {
						$("#savedsettlement").addClass("d_none");
						$("#savingsettlement").addClass("d_none");
						$("#btn-save-settlement").removeClass("d_none");
						toastr.error('Submission error!. Code: ' + xhr.status + ', Details: ' + errorThrown);
					}
				},
                cache: false,
                contentType: false,
                processData: false
            });
        }
        else {
            toastr.error('Please correct the errors on form inputs to save');
        }
    });

    // Have Signatories
    var signatories = $('#ClientSignatoryCount').val();
    if (signatories > 0) {
        $('#SignatoriesDiv').removeClass('d_none');
        $('#SignatoryDiv1').addClass('d_none');
        $('#btn-save-signatories').addClass('d_none');
		$('#btn-edit-signatories').removeClass('d_none'); 
    }
    else {
        $('#SignatoryDiv1').removeClass('d_none');
        $('#btn-save-signatories').removeClass('d_none');
        $('#btn-edit-signatories').addClass('d_none');
    }

    //btn-save-signatories
    $('#btn-save-signatories').on('click', function (e) {
        e.preventDefault();
        if ($('#form').valid()) {
			$("#btn-save-signatories").addClass("d_none");
			$("#savingsignatories").removeClass("d_none");
            var formData = new FormData($("#form")[0]);
            $.ajax({
                url: '/Client/SaveSignatoriesInfo',
                type: 'POST',
                data: formData,
                async: true,
                success: function () {
                    window.setTimeout(close, 500);
                    window.setTimeout(enablesave, 3000);
                    function close() {
						$("#savingsignatories").addClass("d_none");
						$("#savedsignatories").removeClass("d_none");
                        toastr.success('Signatories details saved', { positionClass: 'toast-top-center' });
                    }
					function enablesave() {
						$("#savedsignatories").addClass("d_none");
						$("#btn-save-signatories").removeClass("d_none");
                    }
				},
				error: function (xhr, textStatus, errorThrown) {
					if (textStatus === 'error') {
						$("#savingsignatories").addClass("d_none");
						$("#savedsignatories").addClass("d_none");
						$("#btn-save-signatories").removeClass("d_none");
						toastr.error('Submission error!. Code: ' + xhr.status + ', Details: ' + errorThrown);
					}
				},
                cache: false,
                contentType: false,
                processData: false
            });
        }
        else {
            toastr.error('Please correct the errors on form inputs to save');
        }
    });

    //btn-edit-signatories
    $('#btn-edit-signatories').on('click', function (e) {
        e.preventDefault();
        LoadUpSignatoryModal();
    });

    $('#btnEditSignatories').on('click', function (e) {
		e.preventDefault();
		$('#ClientSignatoryCount').val("0");
		var formData = new FormData($("#form")[0]);
        $.ajax({
            url: '/Client/ClearSignatories',
			type: 'POST',
			data: formData,
            async: true,
            success: function () {
                window.setTimeout(saved, 500);
                function saved() {
                    $('#SignatoryDiv1').removeClass('d_none');
                    $('#btn-save-signatories').removeClass('d_none');
                    $('#btn-edit-signatories').addClass('d_none');
                    $('#SignatoriesDiv').addClass('d_none');
                    $('#btn-save-signatories').removeClass('d_none');
                    $('#LoadUpSignatoryModal').modal('hide');
                }
			},
			error: function (xhr, textStatus, errorThrown) {
				if (textStatus === 'error') {
					toastr.error('Submission error!. Code: ' + xhr.status + ', Details: ' + errorThrown);
				}
			},
            cache: false,
            contentType: false,
            processData: false
        });
    });

    // Have Representatives
	var representatives = $('#DesignatedUserCount').val();
    if (representatives > 0) {
        $('#RepresentativesDiv').removeClass('d_none');
        $('#UserDiv1').addClass('d_none');
        $('#btn-save-representatives').addClass('d_none');
        $('#btn-edit-representatives').removeClass('d_none');
    }
    else {
        $('#RepresentativesDiv').addClass('d_none');
        $('#btn-save-representatives').removeClass('d_none');
        $('#btn-edit-representatives').addClass('d_none');
    }
    
    //btn-edit-representatives
    $('#btn-edit-representatives').on('click', function (e) {
        e.preventDefault();
        LoadUpRepresentativeModal();
    });

    $('#btnEditRepresentatives').on('click', function (e) {
		e.preventDefault();
		$('#DesignatedUserCount').val("0");
		var formData = new FormData($("#form")[0]);
        $.ajax({
            url: '/Client/ClearRepresentatives',
			type: 'POST',
			data: formData,
            async: true,
            success: function () {
                window.setTimeout(saved, 500);
                function saved() {
                    $('#UserDiv1').removeClass('d_none');
                    $('#btn-save-representatives').removeClass('d_none');
                    $('#btn-edit-representatives').addClass('d_none');
                    $('#RepresentativesDiv').addClass('d_none');
                    $('#btn-save-representatives').removeClass('d_none');
                    $('#LoadUpRepresentativeModal').modal('hide');
                }
			},
			error: function (xhr, textStatus, errorThrown) {
				if (textStatus === 'error') {
					toastr.error('Submission error!. Code: ' + xhr.status + ', Details: ' + errorThrown);
				}
			},
            cache: false,
            contentType: false,
            processData: false
        });
    });

    //btn-save-signatories
    $('#btn-save-representatives').on('click', function (e) {
        e.preventDefault();
        if ($('#form').valid()) {
			$("#btn-save-representatives").addClass("d_none");
			$("#savingrepresentatives").removeClass("d_none");
            var formData = new FormData($("#form")[0]);
            $.ajax({
                url: '/Client/SaveRepresentativesInfo',
                type: 'POST',
                data: formData,
                async: true,
                success: function () {
                    window.setTimeout(close, 500);
                    window.setTimeout(enablesave, 3000);
                    function close() {
						$("#savingrepresentatives").addClass("d_none");
						$("#savedrepresentatives").removeClass("d_none");
                        toastr.success('Representatives details saved', { positionClass: 'toast-top-center' });
                    }
					function enablesave() {
						$("#savedrepresentatives").addClass("d_none");
						$("#btn-save-representatives").removeClass("d_none"); 
                    }
				},
				error: function (xhr, textStatus, errorThrown) {
					if (textStatus === 'error') {
						$("#savingrepresentatives").addClass("d_none");
						$("#savedrepresentatives").addClass("d_none");
						$("#btn-save-representatives").removeClass("d_none");
						toastr.error('Submission error!. Code: ' + xhr.status + ', Details: ' + errorThrown);
					}
				},
                cache: false,
                contentType: false,
                processData: false
            });
        }
        else {
            toastr.error('Please correct the errors on form inputs to save');
        }
    }); 

    //Submit Form Action
    $("form#form").submit(function (e) {
        e.preventDefault();
        if ($('#form').valid()) {
            $("#Loading_Div").show('fast');
            $('#form').hide("fast");
            var formData = new FormData($(this)[0]);
            $.ajax({
                url: '/Client/AddNewApplication',
                type: 'POST',
                data: formData,
                async: true,
				success: function (XmlHttpRequest) {
                    window.setTimeout(close, 500);
                    window.setTimeout(close2, 1500);
					function close() {
						if (XmlHttpRequest === 'success') {
							$('#form')[0].reset();
							$("#Loading_Div").hide('fast');
							$('#Success_Div').show("fast");
						}
						else {
							toastr.error(XmlHttpRequest);
						}
					}
					function close2() {
						if (XmlHttpRequest === 'success') {
							$('#Success_Div').hide("fast");
							window.location.replace("/Client/ViewAll");
						}
						else {
							$("#Loading_Div").hide('fast');
							$('#form').show("fast");
						}
                    }
				},
				error: function (xhr, textStatus, errorThrown) {
					if (textStatus === 'error') {
						$("#Loading_Div").hide('fast');
						$('#form').show("fast");
						toastr.error('Submission error!. Code: ' + xhr.status + ', Details: ' + errorThrown);
					}
				},
                cache: false,
                contentType: false,
                processData: false
            });
        }
    });
});