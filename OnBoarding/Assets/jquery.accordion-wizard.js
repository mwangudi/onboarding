;( function( $, window, document, undefined ) {

	"use strict";

	var $steps = [];

	var pluginName = "accWizard",
		defaults = {
			start: 1,
			mode: "wizard", // wizard or edit
			enableScrolling: true,
			scrollPadding: 5,
			autoButtons: true,
			autoButtonsNextClass: null,
			autoButtonsPrevClass: null,
			autoButtonsShowSubmit: true,
			autoButtonsSubmitText: 'Submit',
			autoButtonsEditSubmitText: 'Save',
			stepNumbers: true,
			stepNumberClass: '',
			beforeNextStep: function( currentStep ) { return true; },
			//onSubmit: function( element ) { return true; }
		};

	function Plugin ( element, options ) {
		this.element = element;
		this.settings = $.extend( {}, defaults, options );
		this._defaults = defaults;
		this._name = pluginName;
		this.init();
	}

	// Avoid Plugin.prototype conflicts
	$.extend( Plugin.prototype, {
		init: function() {

			var mthis = this;

			// cache the steps
			this.$steps = $('[data-acc-step]');

			// get the initial acc-step height so we can calculate offset in animation
			this.stepHeight = $('[data-acc-step]').eq(0).outerHeight();

			// step numbers
			if( this.settings.stepNumbers ) {
				this.$steps.each(function(i, el) {
					$('[data-acc-title]', el).prepend('<span class="acc-step-number '+mthis.settings.stepNumberClass+'">' + (i+1) + '</span> ');
				})
			}

			// autobuttons
			if( this.settings.autoButtons ) {
				this.$steps.each(function(i, el) {

					var $content = $('[data-acc-content]', el);

					// Add prev, not on first
					if( i > 0 ) {
						$content.append('<a href="#" class="'+mthis.settings.autoButtonsPrevClass+'" data-acc-btn-prev>Back</a>');
					}

					// Add next, submit on last
					if( i < ( mthis.$steps.length - 1 ) ) {
						$content.append('<a href="#" class="'+mthis.settings.autoButtonsNextClass+'" data-acc-btn-next>Next</a>');
					} else if( mthis.settings.autoButtonsShowSubmit ) {

						var btnText = mthis.settings.mode == 'wizard' ? mthis.settings.autoButtonsSubmitText : mthis.settings.autoButtonsEditSubmitText;

						$content.append('<input type="submit" class="' + mthis.settings.autoButtonsNextClass + '" value="' + btnText + '">');
					}

				})
			}


			// set current
			this.currentIndex = this.settings.start - 1;

			if( this.settings.mode == 'wizard' ) {
				// 'wizard' mode
				this.activateStep( this.currentIndex, true );

                $('[data-acc-btn-next]').on('click', function () {
					if ($('#form').valid()) {

						//Populate Form Summary
						$('#ConfirmCompany').html($('#CompanyName').val());
						$('#ConfirmRegistration').html($('#CompanyRegistration').val());
						var Address = $('#CompanyTownCity').val() + " - " + $('#Building').val() + " - " + $('#Street').val();
						$('#ConfirmAddress').html(Address);

						//Settlement Details
						var chosenvalue = $("#HaveSettlementAccount").val();
						var settlements = $('#SettlementAccountsCount').val();
						if (chosenvalue === "Yes" && settlements > 0) {
							if (settlements === "2") {
								$('#ConfirmKES').html($('#SettlementTable tr').find("td").eq(1).html());
								$('#ConfirmOther').html($('#SettlementTable tr').find("td").eq(3).html());
								$('#ConfirmOthertr3').addClass('d_none');
								$('#ConfirmOthertr4').addClass('d_none');
								$('#ConfirmOthertr5').addClass('d_none');
							}
							else if (settlements === "3") {
								$('#ConfirmKES').html($('#SettlementTable tr').find("td").eq(1).html());
								$('#ConfirmOther').html($('#SettlementTable tr').find("td").eq(3).html());
								$('#ConfirmAccount3').html($('#SettlementTable tr').find("td").eq(5).html());
								$('#ConfirmOthertr4').addClass('d_none');
								$('#ConfirmOthertr5').addClass('d_none');
							}
							else if (settlements === "4") {
								$('#ConfirmKES').html($('#SettlementTable tr').find("td").eq(1).html());
								$('#ConfirmOther').html($('#SettlementTable tr').find("td").eq(3).html());
								$('#ConfirmAccount3').html($('#SettlementTable tr').find("td").eq(5).html());
								$('#ConfirmAccount4').html($('#SettlementTable tr').find("td").eq(7).html());
								$('#ConfirmOthertr5').addClass('d_none');
							}
							else if (settlements === "5") {
								$('#ConfirmKES').html($('#SettlementTable tr').find("td").eq(1).html());
								$('#ConfirmOther').html($('#SettlementTable tr').find("td").eq(3).html());
								$('#ConfirmAccount3').html($('#SettlementTable tr').find("td").eq(5).html());
								$('#ConfirmAccount4').html($('#SettlementTable tr').find("td").eq(7).html());
								$('#ConfirmAccount5').html($('#SettlementTable tr').find("td").eq(9).html());
							}
							else {
								$('#ConfirmKES').html($('#SettlementTable tr').find("td").eq(1).html());
								$('#ConfirmOther').html($('#SettlementTable tr').find("td").eq(1).html());
							}
						}
						else if (chosenvalue === "Yes" && settlements === "0") {
							$('#ConfirmKEStrText').html('Settlement Instruction 1:').val();
							$('#ConfirmKES').html($('#SettlementAccount1').val());
							$('#ConfirmOther').html($('#SettlementAccount2').val());
							if ($('#SettlementAccount3').val() === "") {
								$('#ConfirmOthertr3').addClass('d_none');
							} else {
								$('#ConfirmAccount3').html($('#SettlementAccount3').val());
								$('#ConfirmOthertr3').removeClass('d_none');
							}
							if ($('#SettlementAccount4').val() === "") {
								$('#ConfirmOthertr4').addClass('d_none');
							} else {
								$('#ConfirmAccount4').html($('#SettlementAccount4').val());
								$('#ConfirmOthertr4').removeClass('d_none');
							}
							if ($('#SettlementAccount5').val() === "") {
								$('#ConfirmOthertr5').addClass('d_none');
							} else {
								$('#ConfirmAccount5').html($('#SettlementAccount5').val());
								$('#ConfirmOthertr5').removeClass('d_none');
							}
							$('#ConfirmOthertr').removeClass('d_none');
						}
						else if (chosenvalue === "No") {
							$('#ConfirmKES').html('No Settlement Instructions').val();
							$('#ConfirmKEStrText').html('').val();
							$('#ConfirmOthertr').addClass('d_none');
							$('#ConfirmOthertr3').addClass('d_none');
							$('#ConfirmOthertr4').addClass('d_none');
							$('#ConfirmOthertr5').addClass('d_none');
						}
						else {
							$('#ConfirmKES').html('No Settlement Instructions').val();
						}

						//Signatory Details
						var signatories = $('#ClientSignatoryCount').val();
						//alert(signatories);
						if (signatories === "0") {
							var Signatory1 = $('#SignatorySurname1').val() + " " + $('#SignatoryOtherNames1').val() + " - " + $('#SignatoryDesignation1').val() + " - " + $('#SignatoryEmail1').val() + " - " + $('#SignatoryPhoneNumber1').val();
							var Signatory2 = $('#SignatorySurname2').val() + " " + $('#SignatoryOtherNames2').val() + " - " + $('#SignatoryDesignation2').val() + " - " + $('#SignatoryEmail2').val() + " - " + $('#SignatoryPhoneNumber2').val();
							var Signatory3 = $('#SignatorySurname3').val() + " " + $('#SignatoryOtherNames3').val() + " - " + $('#SignatoryDesignation3').val() + " - " + $('#SignatoryEmail3').val() + " - " + $('#SignatoryPhoneNumber3').val();
							var Signatory4 = $('#SignatorySurname4').val() + " " + $('#SignatoryOtherNames4').val() + " - " + $('#SignatoryDesignation4').val() + " - " + $('#SignatoryEmail4').val() + " - " + $('#SignatoryPhoneNumber4').val();
							var Signatory5 = $('#SignatorySurname5').val() + " " + $('#SignatoryOtherNames5').val() + " - " + $('#SignatoryDesignation5').val() + " - " + $('#SignatoryEmail5').val() + " - " + $('#SignatoryPhoneNumber5').val();
							if ($('#SignatoryEmail1').val() === '') {
								$('#Signatory1tr').addClass('d_none');
							}
							else {
								$('#Signatory1tr').removeClass('d_none');
								$('#ConfirmSignatory1').html(Signatory1);
							}
							if ($('#SignatoryEmail2').val() === '') {
								$('#Signatory2tr').addClass('d_none');
							}
							else {
								$('#Signatory2tr').removeClass('d_none');
								$('#ConfirmSignatory2').html(Signatory2);
							}
							if ($('#SignatoryEmail3').val() === '') {
								$('#Signatory3tr').addClass('d_none');
							}
							else {
								$('#Signatory3tr').removeClass('d_none');
								$('#ConfirmSignatory3').html(Signatory3);
							}
							if ($('#SignatoryEmail4').val() === '') {
								$('#Signatory4tr').addClass('d_none');
							}
							else {
								$('#Signatory4tr').removeClass('d_none');
								$('#ConfirmSignatory4').html(Signatory4);
							}
							if ($('#SignatoryEmail5').val() === '') {
								$('#Signatory5tr').addClass('d_none');
							}
							else {
								$('#Signatory5tr').removeClass('d_none');
								$('#ConfirmSignatory5').html(Signatory5);
							}
						}
						else if (signatories > 0) {
							if (signatories === "1") {
								$('#Signatory1tr').removeClass('d_none');
								$('#ConfirmSignatory1').html("<b>NAMES:</b> " + $('#SignatoriesTable tr').find("td").eq(0).html() + " - <b>DESIGNATION:</b> " + $('#SignatoriesTable tr').find("td").eq(1).html() + " - <b>PHONE:</b> " + $('#SignatoriesTable tr').find("td").eq(3).html() + " - <b>SIGNATURE:</b> " + $('#SignatoriesTable tr').find("td").eq(4).html());
								$('#Signatory2tr').addClass('d_none');
								$('#Signatory3tr').addClass('d_none');
								$('#Signatory4tr').addClass('d_none');
								$('#Signatory5tr').addClass('d_none');
							}
							else if (signatories === "2") {
								$('#Signatory1tr').removeClass('d_none');
								$('#ConfirmSignatory1').html("<b>NAMES:</b> " + $('#SignatoriesTable tr').find("td").eq(0).html() + " - <b>DESIGNATION:</b> " + $('#SignatoriesTable tr').find("td").eq(1).html() + " - <b>EMAIL:</b> " + $('#SignatoriesTable tr').find("td").eq(2).html() + " - <b>SIGNATURE:</b> " + $('#SignatoriesTable tr').find("td").eq(4).html());
								$('#ConfirmSignatory2').html("<b>NAMES:</b> " + $('#SignatoriesTable tr').find("td").eq(5).html() + " - <b>DESIGNATION:</b> " + $('#SignatoriesTable tr').find("td").eq(6).html() + " - <b>PHONE:</b> " + $('#SignatoriesTable tr').find("td").eq(7).html() + " - <b>EMAIL:</b> " + $('#SignatoriesTable tr').find("td").eq(8).html());
								$('#Signatory3tr').addClass('d_none');
								$('#Signatory4tr').addClass('d_none');
								$('#Signatory5tr').addClass('d_none');
							}
							else if (signatories === "3") {
								$('#Signatory1tr').removeClass('d_none');
								$('#ConfirmSignatory1').html("<b>NAMES:</b> " + $('#SignatoriesTable tr').find("td").eq(0).html() + " - <b>DESIGNATION:</b> " + $('#SignatoriesTable tr').find("td").eq(1).html() + " - <b>EMAIL:</b> " + $('#SignatoriesTable tr').find("td").eq(2).html() + " - <b>SIGNATURE:</b> " + $('#SignatoriesTable tr').find("td").eq(4).html());
								$('#ConfirmSignatory2').html("<b>NAMES:</b> " + $('#SignatoriesTable tr').find("td").eq(5).html() + " - <b>DESIGNATION:</b> " + $('#SignatoriesTable tr').find("td").eq(6).html() + " - <b>PHONE:</b> " + $('#SignatoriesTable tr').find("td").eq(7).html() + " - <b>EMAIL:</b> " + $('#SignatoriesTable tr').find("td").eq(8).html());
								$('#ConfirmSignatory3').html("<b>NAMES:</b> " + $('#SignatoriesTable tr').find("td").eq(10).html() + " - <b>DESIGNATION:</b> " + $('#SignatoriesTable tr').find("td").eq(11).html() + " - <b>PHONE:</b> " + $('#SignatoriesTable tr').find("td").eq(12).html() + " - <b>EMAIL:</b> " + $('#SignatoriesTable tr').find("td").eq(13).html());
								$('#Signatory4tr').addClass('d_none');
								$('#Signatory5tr').addClass('d_none');
							}
							else if (signatories === "4") {
								$('#Signatory1tr').removeClass('d_none');
								$('#ConfirmSignatory1').html("<b>NAMES:</b> " + $('#SignatoriesTable tr').find("td").eq(0).html() + " - <b>DESIGNATION:</b> " + $('#SignatoriesTable tr').find("td").eq(1).html() + " - <b>EMAIL:</b> " + $('#SignatoriesTable tr').find("td").eq(2).html() + " - <b>SIGNATURE:</b> " + $('#SignatoriesTable tr').find("td").eq(4).html());
								$('#ConfirmSignatory2').html("<b>NAMES:</b> " + $('#SignatoriesTable tr').find("td").eq(5).html() + " - <b>DESIGNATION:</b> " + $('#SignatoriesTable tr').find("td").eq(6).html() + " - <b>PHONE:</b> " + $('#SignatoriesTable tr').find("td").eq(7).html() + " - <b>EMAIL:</b> " + $('#SignatoriesTable tr').find("td").eq(8).html());
								$('#ConfirmSignatory3').html("<b>NAMES:</b> " + $('#SignatoriesTable tr').find("td").eq(10).html() + " - <b>DESIGNATION:</b> " + $('#SignatoriesTable tr').find("td").eq(11).html() + " - <b>PHONE:</b> " + $('#SignatoriesTable tr').find("td").eq(12).html() + " - <b>EMAIL:</b> " + $('#SignatoriesTable tr').find("td").eq(13).html());
								$('#ConfirmSignatory4').html("<b>NAMES:</b> " + $('#SignatoriesTable tr').find("td").eq(15).html() + " - <b>DESIGNATION:</b> " + $('#SignatoriesTable tr').find("td").eq(16).html() + " - <b>PHONE:</b> " + $('#SignatoriesTable tr').find("td").eq(17).html() + " - <b>EMAIL:</b> " + $('#SignatoriesTable tr').find("td").eq(18).html());
								$('#Signatory5tr').addClass('d_none');
							}
							else if (signatories === "5") {
								$('#Signatory1tr').removeClass('d_none');
								$('#ConfirmSignatory1').html("<b>NAMES:</b> " + $('#SignatoriesTable tr').find("td").eq(0).html() + " - <b>DESIGNATION:</b> " + $('#SignatoriesTable tr').find("td").eq(1).html() + " - <b>EMAIL:</b> " + $('#SignatoriesTable tr').find("td").eq(2).html() + " - <b>SIGNATURE:</b> " + $('#SignatoriesTable tr').find("td").eq(4).html());
								$('#ConfirmSignatory2').html("<b>NAMES:</b> " + $('#SignatoriesTable tr').find("td").eq(5).html() + " - <b>DESIGNATION:</b> " + $('#SignatoriesTable tr').find("td").eq(6).html() + " - <b>PHONE:</b> " + $('#SignatoriesTable tr').find("td").eq(7).html() + " - <b>EMAIL:</b> " + $('#SignatoriesTable tr').find("td").eq(8).html());
								$('#ConfirmSignatory3').html("<b>NAMES:</b> " + $('#SignatoriesTable tr').find("td").eq(10).html() + " - <b>DESIGNATION:</b> " + $('#SignatoriesTable tr').find("td").eq(11).html() + " - <b>PHONE:</b> " + $('#SignatoriesTable tr').find("td").eq(12).html() + " - <b>EMAIL:</b> " + $('#SignatoriesTable tr').find("td").eq(13).html());
								$('#ConfirmSignatory4').html("<b>NAMES:</b> " + $('#SignatoriesTable tr').find("td").eq(15).html() + " - <b>DESIGNATION:</b> " + $('#SignatoriesTable tr').find("td").eq(16).html() + " - <b>PHONE:</b> " + $('#SignatoriesTable tr').find("td").eq(17).html() + " - <b>EMAIL:</b> " + $('#SignatoriesTable tr').find("td").eq(18).html());
								$('#ConfirmSignatory5').html("<b>NAMES:</b> " + $('#SignatoriesTable tr').find("td").eq(20).html() + " - <b>DESIGNATION:</b> " + $('#SignatoriesTable tr').find("td").eq(21).html() + " - <b>PHONE:</b> " + $('#SignatoriesTable tr').find("td").eq(22).html() + " - <b>EMAIL:</b> " + $('#SignatoriesTable tr').find("td").eq(23).html());
							}
							else {
								$('#Signatory1tr').removeClass('d_none');
								$('#Signatory2tr').removeClass('d_none');
								$('#Signatory3tr').removeClass('d_none');
								$('#Signatory4tr').removeClass('d_none');
								$('#Signatory5tr').removeClass('d_none');
							}
						}

						//Representatives Details
						var representatives = $('#DesignatedUserCount').val();
						if (representatives === "0") {
							//User Details
							var User1 = $('#UserSurname1').val() + " " + $('#UserOthernames1').val() + " - " + $('#UserMobileNumber1').val() + " - " + $('#UserEmail1').val() + " - Emarket SignUp: " + $('#EMarketSignUp1').find('option:selected').text() + " -  Transaction Limit " + $('#TransactionLimit1').val();
							var User2 = $('#UserSurname2').val() + " " + $('#UserOthernames2').val() + " - " + $('#UserMobileNumber2').val() + " - " + $('#UserEmail2').val() + " - Emarket SignUp: " + $('#EMarketSignUp2').find('option:selected').text() + " -  Transaction Limit " + $('#TransactionLimit2').val();
							var User3 = $('#UserSurname3').val() + " " + $('#UserOthernames3').val() + " - " + $('#UserMobileNumber3').val() + " - " + $('#UserEmail3').val() + " - Emarket SignUp: " + $('#EMarketSignUp3').find('option:selected').text() + " -  Transaction Limit " + $('#TransactionLimit3').val();
							var User4 = $('#UserSurname4').val() + " " + $('#UserOthernames4').val() + " - " + $('#UserMobileNumber4').val() + " - " + $('#UserEmail4').val() + " - Emarket SignUp: " + $('#EMarketSignUp4').find('option:selected').text() + " -  Transaction Limit " + $('#TransactionLimit4').val();
							var User5 = $('#UserSurname5').val() + " " + $('#UserOthernames5').val() + " - " + $('#UserMobileNumber5').val() + " - " + $('#UserEmail5').val() + " - Emarket SignUp: " + $('#EMarketSignUp5').find('option:selected').text() + " -  Transaction Limit " + $('#TransactionLimit5').val();

							if ($('#UserEmail1').val() === '') {
								$('#AuthUser1tr').addClass('d_none');
							}
							else {
								$('#AuthUser1tr').removeClass('d_none');
								$('#ConfirmUser1').html(User1);
							}
							if ($('#UserEmail2').val() === '') {
								$('#AuthUser2tr').addClass('d_none');
							}
							else {
								$('#AuthUser2tr').removeClass('d_none');
								$('#ConfirmUser2').html(User2);
							}
							if ($('#UserEmail3').val() === '') {
								$('#AuthUser3tr').addClass('d_none');
							}
							else {
								$('#AuthUser3tr').removeClass('d_none');
								$('#ConfirmUser3').html(User3);
							}
							if ($('#UserEmail4').val() ==='') {
								$('#AuthUser4tr').addClass('d_none');
							}
							else {
								$('#AuthUser4tr').removeClass('d_none');
								$('#ConfirmUser4').html(User4);
							}
							if ($('#UserEmail5').val() === '') {
								$('#AuthUser5tr').addClass('d_none');
							}
							else {
								$('#AuthUser5tr').removeClass('d_none');
								$('#ConfirmUser5').html(User5);
							}
						}
						else if (representatives > 0) {
							if (representatives === "1") {
								$('#AuthUser1tr').removeClass('d_none');
								$('#ConfirmUser1').html("<b>NAMES:</b> " + $('#RepresentativesTable tr').find("td").eq(0).html() + " - <b>EMAIL:</b> " + $('#RepresentativesTable tr').find("td").eq(1).html() + " - <b>PHONE:</b> " + $('#RepresentativesTable tr').find("td").eq(2).html() + " - <b>TRADING LIMIT:</b> " + $('#RepresentativesTable tr').find("td").eq(3).html() + " - <b>EMARKET SIGNUP:</b> " + $('#RepresentativesTable tr').find("td").eq(4).html());
								$('#AuthUser2tr').addClass('d_none');
								$('#AuthUser3tr').addClass('d_none');
								$('#AuthUser4tr').addClass('d_none');
								$('#AuthUser5tr').addClass('d_none');
							}
							else if (representatives === "2") {
								$('#ConfirmUser1').html("<b>NAMES:</b> " + $('#RepresentativesTable tr').find("td").eq(0).html() + " - <b>EMAIL:</b> " + $('#RepresentativesTable tr').find("td").eq(1).html() + " - <b>PHONE:</b> " + $('#RepresentativesTable tr').find("td").eq(2).html() + " - <b>TRADING LIMIT:</b> " + $('#RepresentativesTable tr').find("td").eq(3).html() + " - <b>EMARKET SIGNUP:</b> " + $('#RepresentativesTable tr').find("td").eq(4).html());
								$('#ConfirmUser2').html("<b>NAMES:</b> " + $('#RepresentativesTable tr').find("td").eq(5).html() + " - <b>EMAIL:</b> " + $('#RepresentativesTable tr').find("td").eq(6).html() + " - <b>PHONE:</b> " + $('#RepresentativesTable tr').find("td").eq(7).html() + " - <b>TRADING LIMIT:</b> " + $('#RepresentativesTable tr').find("td").eq(8).html() + " - <b>EMARKET SIGNUP:</b> " + $('#RepresentativesTable tr').find("td").eq(9).html());
								$('#AuthUser1tr').removeClass('d_none');
								$('#AuthUser2tr').removeClass('d_none');
								$('#AuthUser3tr').addClass('d_none');
								$('#AuthUser4tr').addClass('d_none');
								$('#AuthUser5tr').addClass('d_none');
							}
							else if (representatives === "3") {
								$('#ConfirmUser1').html("<b>NAMES:</b> " + $('#RepresentativesTable tr').find("td").eq(0).html() + " - <b>EMAIL:</b> " + $('#RepresentativesTable tr').find("td").eq(1).html() + " - <b>PHONE:</b> " + $('#RepresentativesTable tr').find("td").eq(2).html() + " - <b>TRADING LIMIT:</b> " + $('#RepresentativesTable tr').find("td").eq(3).html() + " - <b>EMARKET SIGNUP:</b> " + $('#RepresentativesTable tr').find("td").eq(4).html());
								$('#ConfirmUser2').html("<b>NAMES:</b> " + $('#RepresentativesTable tr').find("td").eq(5).html() + " - <b>EMAIL:</b> " + $('#RepresentativesTable tr').find("td").eq(6).html() + " - <b>PHONE:</b> " + $('#RepresentativesTable tr').find("td").eq(7).html() + " - <b>TRADING LIMIT:</b> " + $('#RepresentativesTable tr').find("td").eq(8).html() + " - <b>EMARKET SIGNUP:</b> " + $('#RepresentativesTable tr').find("td").eq(9).html());
								$('#ConfirmUser3').html("<b>NAMES:</b> " + $('#RepresentativesTable tr').find("td").eq(10).html() + " - <b>EMAIL:</b> " + $('#RepresentativesTable tr').find("td").eq(11).html() + " - <b>PHONE:</b> " + $('#RepresentativesTable tr').find("td").eq(12).html() + " - <b>TRADING LIMIT:</b> " + $('#RepresentativesTable tr').find("td").eq(13).html() + " - <b>EMARKET SIGNUP:</b> " + $('#RepresentativesTable tr').find("td").eq(14).html());
								$('#AuthUser1tr').removeClass('d_none');
								$('#AuthUser2tr').removeClass('d_none');
								$('#AuthUser3tr').removeClass('d_none');
								$('#AuthUser4tr').addClass('d_none');
								$('#AuthUser5tr').addClass('d_none');
							}
							else if (representatives === "4") {
								$('#ConfirmUser1').html("<b>NAMES:</b> " + $('#RepresentativesTable tr').find("td").eq(0).html() + " - <b>EMAIL:</b> " + $('#RepresentativesTable tr').find("td").eq(1).html() + " - <b>PHONE:</b> " + $('#RepresentativesTable tr').find("td").eq(2).html() + " - <b>TRADING LIMIT:</b> " + $('#RepresentativesTable tr').find("td").eq(3).html() + " - <b>EMARKET SIGNUP:</b> " + $('#RepresentativesTable tr').find("td").eq(4).html());
								$('#ConfirmUser2').html("<b>NAMES:</b> " + $('#RepresentativesTable tr').find("td").eq(5).html() + " - <b>EMAIL:</b> " + $('#RepresentativesTable tr').find("td").eq(6).html() + " - <b>PHONE:</b> " + $('#RepresentativesTable tr').find("td").eq(7).html() + " - <b>TRADING LIMIT:</b> " + $('#RepresentativesTable tr').find("td").eq(8).html() + " - <b>EMARKET SIGNUP:</b> " + $('#RepresentativesTable tr').find("td").eq(9).html());
								$('#ConfirmUser3').html("<b>NAMES:</b> " + $('#RepresentativesTable tr').find("td").eq(10).html() + " - <b>EMAIL:</b> " + $('#RepresentativesTable tr').find("td").eq(11).html() + " - <b>PHONE:</b> " + $('#RepresentativesTable tr').find("td").eq(12).html() + " - <b>TRADING LIMIT:</b> " + $('#RepresentativesTable tr').find("td").eq(13).html() + " - <b>EMARKET SIGNUP:</b> " + $('#RepresentativesTable tr').find("td").eq(14).html());
								$('#ConfirmUser4').html("<b>NAMES:</b> " + $('#RepresentativesTable tr').find("td").eq(15).html() + " - <b>EMAIL:</b> " + $('#RepresentativesTable tr').find("td").eq(16).html() + " - <b>PHONE:</b> " + $('#RepresentativesTable tr').find("td").eq(17).html() + " - <b>TRADING LIMIT:</b> " + $('#RepresentativesTable tr').find("td").eq(18).html() + " - <b>EMARKET SIGNUP:</b> " + $('#RepresentativesTable tr').find("td").eq(19).html());
								$('#AuthUser1tr').removeClass('d_none');
								$('#AuthUser2tr').removeClass('d_none');
								$('#AuthUser3tr').removeClass('d_none');
								$('#AuthUser4tr').removeClass('d_none');
								$('#AuthUser5tr').addClass('d_none');
							}
							else if (representatives === "5") {
								$('#ConfirmUser1').html("<b>NAMES:</b> " + $('#RepresentativesTable tr').find("td").eq(0).html() + " - <b>EMAIL:</b> " + $('#RepresentativesTable tr').find("td").eq(1).html() + " - <b>PHONE:</b> " + $('#RepresentativesTable tr').find("td").eq(2).html() + " - <b>TRADING LIMIT:</b> " + $('#RepresentativesTable tr').find("td").eq(3).html() + " - <b>EMARKET SIGNUP:</b> " + $('#RepresentativesTable tr').find("td").eq(4).html());
								$('#ConfirmUser2').html("<b>NAMES:</b> " + $('#RepresentativesTable tr').find("td").eq(5).html() + " - <b>EMAIL:</b> " + $('#RepresentativesTable tr').find("td").eq(6).html() + " - <b>PHONE:</b> " + $('#RepresentativesTable tr').find("td").eq(7).html() + " - <b>TRADING LIMIT:</b> " + $('#RepresentativesTable tr').find("td").eq(8).html() + " - <b>EMARKET SIGNUP:</b> " + $('#RepresentativesTable tr').find("td").eq(9).html());
								$('#ConfirmUser3').html("<b>NAMES:</b> " + $('#RepresentativesTable tr').find("td").eq(10).html() + " - <b>EMAIL:</b> " + $('#RepresentativesTable tr').find("td").eq(11).html() + " - <b>PHONE:</b> " + $('#RepresentativesTable tr').find("td").eq(12).html() + " - <b>TRADING LIMIT:</b> " + $('#RepresentativesTable tr').find("td").eq(13).html() + " - <b>EMARKET SIGNUP:</b> " + $('#RepresentativesTable tr').find("td").eq(14).html());
								$('#ConfirmUser4').html("<b>NAMES:</b> " + $('#RepresentativesTable tr').find("td").eq(15).html() + " - <b>EMAIL:</b> " + $('#RepresentativesTable tr').find("td").eq(16).html() + " - <b>PHONE:</b> " + $('#RepresentativesTable tr').find("td").eq(17).html() + " - <b>TRADING LIMIT:</b> " + $('#RepresentativesTable tr').find("td").eq(18).html() + " - <b>EMARKET SIGNUP:</b> " + $('#RepresentativesTable tr').find("td").eq(19).html());
								$('#ConfirmUser5').html("<b>NAMES:</b> " + $('#RepresentativesTable tr').find("td").eq(20).html() + " - <b>EMAIL:</b> " + $('#RepresentativesTable tr').find("td").eq(21).html() + " - <b>PHONE:</b> " + $('#RepresentativesTable tr').find("td").eq(22).html() + " - <b>TRADING LIMIT:</b> " + $('#RepresentativesTable tr').find("td").eq(23).html() + " - <b>EMARKET SIGNUP:</b> " + $('#RepresentativesTable tr').find("td").eq(24).html());
								$('#AuthUser1tr').removeClass('d_none');
								$('#AuthUser2tr').removeClass('d_none');
								$('#AuthUser3tr').removeClass('d_none');
								$('#AuthUser4tr').removeClass('d_none');
								$('#AuthUser5tr').removeClass('d_none');
							}
						}
						else {
							$('#AuthUser1tr').addClass('d_none');
							$('#AuthUser2tr').addClass('d_none');
							$('#AuthUser3tr').addClass('d_none');
							$('#AuthUser4tr').addClass('d_none');
							$('#AuthUser5tr').addClass('d_none');
						}

					    if( mthis.settings.beforeNextStep( mthis.currentIndex + 1 ) ) {
						    mthis.activateStep( mthis.currentIndex + 1 );
						}
                    }
                    else {
                        $(function () {
                            toastr.error('Please correct the errors on form inputs to proceed', { positionClass: 'toast-top-center' });
                        });
                    }
				});

				$('[data-acc-btn-prev]').on('click', function() {
					mthis.activateStep( mthis.currentIndex - 1 );
				});

			} else if ( this.settings.mode == 'edit' ) {
				// 'edit' mode

				this.activateAllSteps();
				$('[data-acc-btn-next]').hide();
				$('[data-acc-btn-prev]').hide();

			}

			/*/ onsubmit
			$(this.element).on('submit', function(e) {
				var resp = mthis.settings.onSubmit();
				if( !resp )
					e.preventDefault();
			});*/

		},
		deactivateStep: function(index, disableScrollOverride) {
			this.$steps.eq(index).removeClass('active');
		},
		activateStep: function(index, disableScrollOverride) {

			this.$steps.removeClass('open');

			var offset = index > this.currentIndex ? this.stepHeight : -this.stepHeight;

			if( !disableScrollOverride && this.settings.enableScrolling ) {
			    $('html').animate({
			        scrollTop: this.$steps.eq(this.currentIndex).offset().top + ( offset - this.settings.scrollPadding )
			    }, 500);
			}

	    	//$('.collapse', this.$steps.eq(index)).stop().collapse('show');
	    	$('[data-acc-content]', this.element).slideUp();

			this.$steps.eq(index)
				.addClass('open')
				.find('[data-acc-content]').slideDown();

			this.currentIndex = index;
		},
		activateNextStep: function() {
			this.activateStep( this.currentIndex + 1 );
		},
		activateAllSteps: function() {
			this.$steps.addClass('open');
			$('[data-acc-content]', this.element).show();
		}
	});

	// wrap up
	$.fn[ pluginName ] = function( options ) {
		return this.each( function() {
			if ( !$.data( this, "plugin_" + pluginName ) ) {
				$.data( this, "plugin_" +
					pluginName, new Plugin( this, options ) );
			}
		} );
	};

} )( jQuery, window, document );