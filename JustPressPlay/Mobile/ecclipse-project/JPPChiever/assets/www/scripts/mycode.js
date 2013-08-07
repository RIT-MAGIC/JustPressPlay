/*
 * 1 - Standalone "Pass to Progress" SWF and APP
 * 2 - Verify Mac Version of PhoneGap
 * 3 - Working from LIVE Achievement data
 * 4 - Pending/Offline stuff
 */


var SITE_ROOT = "https://play.rit.edu/";
var SALT = "fancyprancing";

var USERNAME = "";
var PASSWORD = "";
var VERIFIED = false;

var CURRENT_CHIEVE_ID = -1;
var CURRENT_CHIEVE_NAME = "none";

var ACHIEVEMENTS = {};

document.addEventListener("deviceready", onDeviceReady, false);

function onDeviceReady()
{
	readFile("remember.txt", decideStatus);
}

function logout()
{
	writeUP("");
	$.mobile.changePage("#loginPage", {transition:"slide"});
}

/*var mario_coin;
var power_up;
var try_again;
var loz_thing;*/

// run every time a page is loaded
$(document).bind('pageinit', function(){
	/*if(!(mario_coin))
	{
		//alert("load");
		
	}*/
});

function decideStatus(text)
{
	if(text == "")
	{
		$.mobile.changePage("#loginPage", {transition:"slide"});
	}
	else
	{
		var up = jQuery.parseJSON( text );
		if(up.u != undefined && up.p != undefined)
		{
			USERNAME = up.u;
			PASSWORD = up.p;
			$.mobile.changePage("#menuPage", {transition:"slide"});
		}
		else
		{
			$.mobile.changePage("#loginPage", {transition:"slide"});
		}
	}
}


$(document).delegate("#loadingPage", "pageshow", function(){
	// just chill here
});

$(document).delegate("#loginPage", "pageshow", function(){
	$("#loginError").html("");
});

function outputToLog(text)
{
	$("#logs").html("Log:\n"+text);
}

$(document).delegate("#logsPage", "pageshow", function(){
	readFile("log.txt", outputToLog);
});

$(document).delegate("#achievementsPage", "pageshow", function(){
	loadChieves();
});

$(document).delegate("#offlineChievePage", "pageshow", function(){
	$("#createChieveError").html("");
});

$(document).delegate("#scanPage", "pageshow", function(){
	
	var $page = $("#scanPage");
	var $header = $page.children(":jqmData(role=content)");
	$header.find("h1").html(CURRENT_CHIEVE_NAME);
	
});

/*$(document).delegate("#giveCardDialog", "pageshow", function(){
	if(!loz_thing){ loz_thing = new Media("/android_asset/www/media/audio/LOZ_Fanfare.mp3", function(){}, function(){}); }
	loz_thing.play();
});


$(document).delegate("#repsDialog", "pageshow", function(){
	if(!mario_coin){ mario_coin = new Media("/android_asset/www/media/audio/marioCoin.mp3", function(){}, function(){}); }
	mario_coin.play();
});


$(document).delegate("#threshDialog", "pageshow", function(){
	if(!power_up){ power_up = new Media("/android_asset/www/media/audio/smb3_power-up.mp3", function(){}, function(){}); }
	power_up.play();
});


$(document).delegate("#alreadyGotDialog", "pageshow", function(){
	if(!try_again){ try_again = new Media("/android_asset/www/media/audio/tryAgain.mp3", function(){}, function(){}); }
	try_again.play();
});*/

function loadChieves()
{
	var dataObj = {};
	dataObj.caretakerUsername = USERNAME;
	dataObj.caretakerPassword = PASSWORD;
	
	var qString = SALT + "?caretakerPassword="+dataObj.caretakerPassword+"&caretakerUsername="+dataObj.caretakerUsername;
	dataObj.authHash = b64_sha256( qString );
	
	$.ajax({
		type: 'POST',
		url: SITE_ROOT+'Chiever/GetAchievements',
		data: dataObj,
		success: getChievesSuccess,
		dataType: "json"
	});
}

function getChievesSuccess(data)
{
	ACHIEVEMENTS = data;
	buildChieves();
}

function buildChieves()
{	
	var $page = $("#achievementsPage");
	
	var $content = $page.children(":jqmData(role=content)");
	
	var newChieves = '';//'<div><a href="#offlineChievePage" data-role="button" class="chieveButton ui-disabled">Create Offline Achievement</a></div>';
	newChieves += '<div><ul data-role="listview">';
	
	var chieves = ACHIEVEMENTS.achievements;
	
	for(var i = 0; i < chieves.length; i++)
	{
		newChieves += '<li><a href="#scanPage" onclick="chieveButtonClick('+chieves[i].aID+')">'+chieves[i].name+'</a></li>';
	}
	
	newChieves += "</ul></div>";
	
	$content.html(newChieves);
	$content.find(":jqmData(role=button)").button();
	$content.find(":jqmData(role=listview)").listview();
	$page.page();
}

function chieveButtonClick(aid)
{
	var chieves = ACHIEVEMENTS.achievements;
	
	for(var i = 0; i < chieves.length; i++)
	{
		if(chieves[i].aID == aid)
		{
			CURRENT_CHIEVE_NAME = chieves[i].name;
			CURRENT_CHIEVE_ID = aid;
		}
	}
}

function signin()
{

	var u = $("#username").val();
	var p = $("#password").val();
	
	if(	u != "" && u != "Username" && u != "Username:" &&
		p != "" && p != "Password" && p != "Password:")
	{
		var dataObj = {};
		dataObj.caretakerUsername = u;
		dataObj.caretakerPassword = p;
		
		var qString = SALT + "?caretakerPassword="+dataObj.caretakerPassword+"&caretakerUsername="+dataObj.caretakerUsername;
		dataObj.authHash = b64_sha256( qString );
		
		USERNAME = u;
		PASSWORD = p;
		
		$.ajax({
			type: 'POST',
			url: SITE_ROOT+'Chiever/Login',
			data: dataObj,
			success: loginSuccess,
			dataType: "json"
		});
		
		$("#loginButton").button('disable');
	}
	
	return false;
}

function loginSuccess(data)
{
	if(data.success == true)
	{
		VERIFIED = true;
		// save u and p into text file...
		
		var up = '{"u":"'+USERNAME+'", "p":"'+PASSWORD+'"}';
		writeUP(up);
		
		$("#loginError").html(data.message);
		$.mobile.changePage("#menuPage", {transition:"slide"});
	}
	else
	{
		VERIFIED = false;
		$("#loginError").html(data.message);
	}
	
	$("#loginButton").button('enable');
}

function scanit()
{
	window.plugins.barcodeScanner.scan(scanSuccess, scanFail);
}

function punchit()
{
	var obj = {};
	obj.text = "https://play.rit.edu/Profile/"+$("#manual_uid").val();
	scanSuccess(obj);
}

function scanSuccess(result)
{
	if(result.text.indexOf("https://play.rit.edu/Profile/") != -1)
	{
		//we'll call this a valid scan
		var playerID = parseInt(result.text.substr(result.text.lastIndexOf("/") + 1));
		var date = new Date();
		var time = ""+date.getUTCFullYear()+"-"+(date.getUTCMonth()+1)+"-"+date.getUTCDate()+" "+date.getUTCHours()+":"+date.getUTCMinutes()+":"+date.getUTCSeconds()+"Z";
		
		//alert(""+playerID+" "+time);
		
		writeLog('{"aID": '					+ CURRENT_CHIEVE_ID+
			    ', "caretakerUsername": '	+ '"' + USERNAME + '"'+
			    //', "caretakerPassword": '	+ '"' + PASSWORD + '"'+
			    ', "hasCardToGive": '		+ '"' + $("#has-cards").val() + '"' +
			    ', "timeScanned": '			+ '"' + time + '"' +
			    ', "userID": '				+ playerID +
			    ', "wasOffline": '			+ '"' + "false" + '"' +
			    ', "OPCODE": '				+ '"SCAN_PLAYER"},\n');		
		
		reportScan(CURRENT_CHIEVE_ID, USERNAME, PASSWORD, $("#has-cards").val(), time, playerID, false, onlineScanReportSuccess);

	}
	else
	{
		$.mobile.changePage(resultDialogs[7]);
	}
}

function scanFail(error)
{
	alert("Scanning failed: " + error);
}

function reportScan(chieveID, user, pass, hasCards, time, playerID, offline, onSuccess)
{
	var dataObj = {};
	dataObj.aID = chieveID;
	dataObj.caretakerUsername = user;
	dataObj.caretakerPassword = pass;
	dataObj.hasCardToGive = hasCards;
	dataObj.timeScanned = time;
	dataObj.userID = playerID;
	dataObj.wasOffline = offline;
	
	var qString = SALT +
					"?aID="+dataObj.aID+
					"&caretakerPassword="+dataObj.caretakerPassword+
					"&caretakerUsername="+dataObj.caretakerUsername+
					"&hasCardToGive="+dataObj.hasCardToGive+
					"&timeScanned="+dataObj.timeScanned+
					"&userID="+dataObj.userID+
					"&wasOffline="+dataObj.wasOffline;
	dataObj.authHash = b64_sha256( qString );
	
	$.ajax({
		type: 'POST',
		url: SITE_ROOT+'Chiever/ReportScan',
		data: dataObj,
		success: onSuccess,
		dataType: "json"
	});
}



var resultDialogs = ["#giveCardDialog", // yes
               "#repsDialog", // cannot test (day limit)
               "#threshDialog", // cannot test (day limit)
               "#officeOnlineDialog", // yes 
               "#offlineDialog", // never
               "#alreadyGotDialog", // yes
               "#tooSoonDialog", // yes
               "#badPlayerDialog", // yes
               "#badCaretakerDialog", //
               "#whatAgainDialog"]; //

function onlineScanReportSuccess(data)
{
	if(data.resultCode < resultDialogs.length)
	{
		$.mobile.changePage(resultDialogs[data.resultCode]);
	}
	else
	{
		alert(data.message);
	}
}