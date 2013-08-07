/*
var $cache = $('.stickyPanel');
var navHeight = $('.top-bar').height() + 20;
var distance = $cache.offset().top;
var scrollThreshold = distance - navHeight;

function fixDiv() {
    



    if ($cache.length > 0) {
        //var distance = $cache.length ? $cache.offset().top : 0,
        //$window = $(window);
        
        //alert(navHeight + "   " + distance + "  " + scrollThreshold);
        //alert(distance);
        //var offst = this.outerHeight($('.top-bar'));
       // var offst = 0;

        if ($(window).scrollTop() >= scrollThreshold ) {
            $cache.css({ 'position': 'fixed', 'top': navHeight });
            //$('body').css('padding-top', offst);
        }
        else {
            $cache.css({ 'position': 'relative', 'top': 'auto' });
            //alert('Shiiiiiiiiiit');
            //$('body').css('padding-top', '0');
        }
    }
}
$(window).scroll(fixDiv);
fixDiv();
*/



var currStoryId = "#storyBL";
var nextId = "";




var arrayBL = new Array(4);
arrayBL[0] = "Brandon";
arrayBL[1] = "6/1/2013";
arrayBL[2] = "images/storyImage.jpg";
arrayBL[3] = "<p>Bacon ipsum dolor sit amet short ribs t-bone ham turkey ham hock leberkas. Andouille beef pig filet mignon corned beef frankfurter capicola leberkas. Ground round tongue pancetta brisket drumstick short ribs rump chicken hamburger frankfurter bresaola sausage ball tip. Pork belly pastrami brisket boudin, hamburger corned beef cow pork chop biltong capicola salami meatball. Bresaola shank doner fatback ground round leberkas kielbasa jowl. Ham chicken tongue tri-tip drumstick.</p><p>Boudin sausage capicola pork ground round frankfurter sirloin filet mignon ham t-bone. Pork loin corned beef fatback meatloaf leberkas capicola. Pig meatloaf bacon hamburger. Bacon doner spare ribs cow turkey kielbasa pork loin tenderloin tri-tip jerky swine tongue.</p>";

var arrayDJ = new Array(4);
arrayDJ[0] = "Dan";
arrayDJ[1] = "5/1/2013";
arrayDJ[2] = null;
arrayDJ[3] = "<p>Bacon ipsum dolor sit amet short ribs t-bone ham turkey ham hock leberkas. Andouille beef pig filet mignon corned beef frankfurter capicola leberkas. Ground round tongue pancetta brisket drumstick short ribs rump chicken hamburger frankfurter bresaola sausage ball tip. Pork belly pastrami brisket boudin, hamburger corned beef cow pork chop biltong capicola salami meatball. Bresaola shank doner fatback ground round leberkas kielbasa jowl. Ham chicken tongue tri-tip drumstick.</p><p>Boudin sausage capicola pork ground round frankfurter sirloin filet mignon ham t-bone. Pork loin corned beef fatback meatloaf leberkas capicola. Pig meatloaf bacon hamburger. Bacon doner spare ribs cow turkey kielbasa pork loin tenderloin tri-tip jerky swine tongue.</p>";

var arrayLL = new Array(4);
arrayLL[0] = "Liz";
arrayLL[1] = "4/1/2013";
arrayLL[2] = null;
arrayLL[3] = null;

var arrayBS = new Array(4);
arrayBS[0] = "Ben";
arrayBS[1] = "5/1/2013";
arrayBS[2] = "images/storyImage.jpg";
arrayBS[3] = null;

var currStory = new Array(4);
currStory = arrayBL;


var achievementBL = new Array(7);
achievementBL[0] = "Brandon Littell";
achievementBL[1] = "For The Lawls";
achievementBL[2] = "2/8/2013";
achievementBL[3] = "images/brandon.png";
achievementBL[4] = "images/leaftifa.png";
achievementBL[5] = "";
achievementBL[6] = '<div class="postBody"><p>Bacon ipsum dolor sit amet nulla ham qui sint exercitation eiusmod commodo, chuck duis velit. Aute in reprehenderit Bacon ipsum dolor sit amet nulla ham qui sint exercitation eiusmod commodo, chuck duis velit. Aute in reprehenderit.</p><hr /><div class="commentsContainer"><h5>2 Comments</h5><div class="comment"><div class="userPhoto"><a href="#"><img src="images/people/0.png" /></a></div><div class="commentBody"><h2><a href="#">Ben Snyder</a> - 5/29/2013</h2><p>Bacon ipsum dolor sit amet nulla ham qui sint exercitation eiusmod commodo, chuck duis velit. Aute in reprehenderit</p></div></div><div class="comment"><div class="userPhoto"><a href="#"><img src="images/people/1.png" /></a></div><div class="commentBody"><h2><a href="#">Daniel Jost</a> - 5/29/2013</h2><p>Odio keffiyeh Schlitz, ex polaroid before they sold out four loko fa excepteur nostrud. Deep v 8-bit aliquip jean shorts squid reprehenderit ethnic. Est church-key bicycle rights, try-hard flexitarian veniam raw denim meh. Ugh gastropub Wes Anderson, pitchfork whatever pop-up kitsch placeat voluptate fixie VHS McSweeney\'s esse mixtape locavore.</p></div></div><hr /><div class="writeComment"><div class="userPhoto hide-for-small"><img src="images/brandon.png" /></div><div class="commentInput"><input type="text" id="commentInput" placeholder="Write a comment..." /></div></div></div></div>';


var achievementOther = new Array(7);
achievementOther[0] = "Joe Pietruch";
achievementOther[1] = "For The Lawls";
achievementOther[2] = "3/15/2013";
achievementOther[3] = "images/people/15.png";
achievementOther[4] = "images/story3.png";
achievementOther[5] = "";
achievementOther[6] = '<div class="postBody"><p>On facebook, David Ira Schwartz writes "is Schwartzing Jennifer DuBrava Hinton."</p><p>Since neither would tell me what that meant, I made my own meaning, which I posted with a comment, "See, but if you don\'t tell me, my brain is just going to pick something and assume \'s what it means. For instance, I currently imagine it to be something like this:" (see attached image).</p><p>In response, Liz writes "I cannot like that photo enough. Srsly." and "I am laughing so hard I\'m crying." Et viola!</p><hr /><div class="commentsContainer"><h5>2 Comments</h5><div class="comment"><div class="userPhoto"><a href="#"><img src="images/people/0.png" /></a></div><div class="commentBody"><h2><a href="#">Ben Snyder</a> - 5/29/2013</h2><p>Bacon ipsum dolor sit amet nulla ham qui sint exercitation eiusmod commodo, chuck duis velit. Aute in reprehenderit</p></div></div><div class="comment"><div class="userPhoto"><a href="#"><img src="images/people/1.png" /></a></div><div class="commentBody"><h2><a href="#">Daniel Jost</a> - 5/29/2013</h2><p>Odio keffiyeh Schlitz, ex polaroid before they sold out four loko fa excepteur nostrud. Deep v 8-bit aliquip jean shorts squid reprehenderit ethnic. Est church-key bicycle rights, try-hard flexitarian veniam raw denim meh. Ugh gastropub Wes Anderson, pitchfork whatever pop-up kitsch placeat voluptate fixie VHS McSweeney\'s esse mixtape locavore.</p></div></div><hr /><div class="writeComment"><div class="userPhoto hide-for-small"><img src="images/brandon.png" /></div><div class="commentInput"><input type="text" id="commentInput" placeholder="Write a comment..." /></div></div></div></div>';

var achievementNone = new Array(7);
achievementNone[0] = "Warlock";
achievementNone[1] = "For The Lawls";
achievementNone[2] = "5/28/2013";
achievementNone[3] = "images/people/1.png";
achievementNone[4] = "";
achievementNone[5] = "";
achievementNone[6] = '<div class="postBody"><p>Warlock hasn\'t posted a story yet! Let him know you\'d like one below!</p><hr /><div class="commentsContainer"><h5>0 Comments</h5><hr /><div class="writeComment"><div class="userPhoto hide-for-small"><img src="images/brandon.png" /></div><div class="commentInput"><input type="text" id="commentInput" placeholder="Write a comment..." /></div></div></div></div>';


var questBL = "";

                    

var authorName = "";
var authorPhoto = "";
var storyAchievement = "";
var storyDate = "";
var storyImage = "";
var storyLink = "";
var storyBody = "";



var currStoryName = "";
var currStoryDate = "";
var currStoryImageSrc = "";
var currStoryText = "";

function loadStory(personId) {
    /*
    var filePath = "data/" + personId + ".js";

    // Read in story values
    $.getJSON(filePath,
        function (json)
        {

            alert("File loaded!");

            var storyArray = new Array(4);
            var count = 0;

            // Save the values to an array
            $.each(json, function (key, val) {
                alert(val);
                storyArray[count++] = val;
            });

            // Store data globally
            currStoryName = storyArray[0];
            currStoryDate = storyArray[1];
            currStoryImageSrc = storyArray[2];
            currStoryText = storyArray[3];
        });
        */

    // This code is meant to die in a fire.
    if (personId == "BL" || personId == "personalInfo") {
        currStoryName = arrayBL[0];
        currStoryDate = arrayBL[1];
        currStoryImageSrc = arrayBL[2];
        currStoryText = arrayBL[3];
    }
    else if (personId == "DJ") {
        currStoryName = arrayDJ[0];
        currStoryDate = arrayDJ[1];
        currStoryImageSrc = arrayDJ[2];
        currStoryText = arrayDJ[3];
    }
    else if (personId == "LL") {
        currStoryName = arrayLL[0];
        currStoryDate = arrayLL[1];
        currStoryImageSrc = arrayLL[2];
        currStoryText = arrayLL[3];
    }
    else if (personId == "BS") {
        currStoryName = arrayBS[0];
        currStoryDate = arrayBS[1];
        currStoryImageSrc = arrayBS[2];
        currStoryText = arrayBS[3];
    }
}


function loadAchievement(id) {
    if (id == "bl") {
        authorName = achievementBL[0];
        authorPhoto = achievementBL[3];
        storyAchievement = achievementBL[1];
        storyDate = achievementBL[2];
        storyImage = achievementBL[4];
        storyLink = achievementBL[5];
        storyBody = achievementBL[6];
    }
    else if (id == "other") {
        authorName = achievementOther[0];
        authorPhoto = achievementOther[3];
        storyAchievement = achievementOther[1];
        storyDate = achievementOther[2];
        storyImage = achievementOther[4];
        storyLink = achievementOther[5];
        storyBody = achievementOther[6];
    }
    else if (id == "none") {
        authorName = achievementNone[0];
        authorPhoto = achievementNone[3];
        storyAchievement = achievementNone[1];
        storyDate = achievementNone[2];
        storyImage = achievementNone[4];
        storyLink = achievementNone[5];
        storyBody = achievementNone[6];
    }

    buildStoryHTML();
}

function loadQuest(id) {

    var TIMELINE_FEED = document.getElementById("loadQuestsHere");

    var newStory = document.createElement('div');
    var html = "";

    if (id = "bl") {
        html += questBL;
        //TIMELINE_FEED.innerHTML = questBL;
    }


    TIMELINE_FEED.innerHTML = '';
   
    newStory.innerHTML = html;
    TIMELINE_FEED.appendChild(newStory.lastChild);
    //TIMELINE_FEED.replaceChild(newStory.firstChild, TIMELINE_FEED.firstChild);
}

function buildStoryHTML() {
    var TIMELINE_FEED = document.getElementById("timelineFeed");
    var recievedHeader = '';
    var storyHeader = '';
    var html = '<div class="timelinePost">';

    // Add photo or link first
    if ( storyImage != "") {
        html += '<div class="postPhoto">' +
                    '<img src="' + storyImage + '" />' +
                '</div>';
    }

    if (storyLink != "") {
        html += '<div class="postLink">' +
                    '<p><a href="#">' + storyLink + '</a></p>' +
                '</div>';
    }

    // Add story info
    html += '<div class="postInfo">' +
                '<div class="userPhoto">' +
                    '<a href="#"><img src="' + authorPhoto + '" /></a>' +
                '</div>' +
                '<h1><a href="#">' + authorName + '</a></h1>' +
                '<h2>Earned <a href="#">' + storyAchievement + '</a></h2>' +
                '<h3>' + storyDate + '</h3>' +
            '</div>';

    // Add body
    html += storyBody;

    // SOMEBODY SHOULD SHOOT ME
    // I'M CODE HITLER
    var newStory = document.createElement('div');
    newStory.innerHTML = html;
    TIMELINE_FEED.replaceChild(newStory.firstChild, TIMELINE_FEED.firstChild);
}

function buildCurrStoryHTML() {
    var TIMELINE_FEED = document.getElementById("timelineFeed");
    var recievedHeader = '';
    var storyHeader = '';
    var html = '<div class="timelinePost">'
                    '<div class="onerow">' +
                        '<div class="storyContent col9">';


    if (nextId == "personalInfo") {
        storyHeader = '<h1>Your Story</h1>';
        recievedHeader = '<h2>' + currStoryDate + '</h2>';
    }
    else {
        storyHeader = '<h1>' + currStoryName + '\'s Story</h1>';
        recievedHeader = '<h2>' + currStoryDate + '</h2>';
    }

    html += storyHeader + recievedHeader;

    if (currStoryImageSrc != null)
        html += "<img src=\"" + currStoryImageSrc + "\" />";

    if (currStoryText != null) {
        html += currStoryText;
    }

    if (currStoryImageSrc == null && currStoryText == null)
        html += "<p>" + currStoryName + " hasn't added a story yet. Leave a comment below to let her know you would like to hear more!</p>";

    html += '</div>' +
            '<div class="storyInfo col3 last">' +
                '<h1>Story Info</h1>';

    if (nextId == "DJ") {
        html += '<h2>Other Earnings</h2>' +
                '<div class="repeatStory hint--top hint--rounded" data-hint="1/3/2013"></div>' +
                '<div class="repeatStory hint--top hint--rounded" data-hint="12/17/2012"></div>' +
                '<div class="repeatStory hint--top hint--rounded" data-hint="12/13/2012"></div>' +
                '<div class="repeatStory hint--top hint--rounded" data-hint="11/29/2012"></div>' +
                '<br /><br />';
    }

    html += '<h2>Part of</h2>' +
            '<a href="#">Quest Name</a>';




    html += '</div></div>' +
            '<div class="onerow">' +
            '<div class="comments col12">Comments</div>' +
            '</div></div>';


    //STORY_CONTAINER.removeChild(STORY_CONTAINER.firstChild);
    var newStory = document.createElement('div');
    newStory.innerHTML = html;
    STORY_CONTAINER.replaceChild(newStory.firstChild, STORY_CONTAINER.firstChild);
}

// QR GENERATION
var create_qrcode = function (text, typeNumber, errorCorrectLevel, table) {

    var qr = qrcode(typeNumber || 4, errorCorrectLevel || 'M');
    qr.addData(text);
    qr.make();

    //	return qr.createTableTag();
    return qr.createImgTag();
};


$(document).ready(function () {


    /***************************
        SITE-WIDE FEATURES
    ***************************/

    FastClick.attach(document.body);


    


    // NAVIGATION DROPDOWNS

    var activeDropdownId = "";

    $('body').click(function (e) {

        if ($(e.target).closest('.dropdownContainer').length > 0)
        {   // Click on dropdown
            //e.stopPropigation();
            return;
        }


        if ($(e.target).closest('[data-dropdownid]').length > 0)
        {   // Click on dropdown button
            var parentButton = $(e.target).closest('[data-dropdownid]');
            var targetId = parentButton.data('dropdownid');

            e.preventDefault();

            $('#' + targetId).toggleClass('active');

            if (targetId == activeDropdownId)
                activeDropdownId = "";
            else
            {
                $('#' + activeDropdownId).toggleClass('active', false);
                activeDropdownId = targetId;
            }


            return;
        }
           

        //alert('About to hide yo shit');
        $('#' + activeDropdownId).toggleClass('active', false);
        activeDropdownId = "";

    });

    // END NAVIGATION DROPDOWNS



    // SCROLL BOXES
    
    $('.scroll').slimScroll();

    $('.dropdownScroll').slimScroll({
        height: '250px',
        size: '6px',
        position: 'right',
        color: '#000',
        alwaysVisible: false,
        distance: '1px',
        railVisible: false,
        railDraggable: true,
        railColor: '#222',
        railOpacity: 0.3,
        wheelStep: 10,
        allowPageScroll: false,
        disableFadeOut: false
    });





    // SERIOUSLY, THIS IS BAD
    // CODE HITLER HAS STRUCK AGAIN

    $(".loadAchievementBL").click(function () {

        loadAchievement('bl');
    });
    $(".loadAchievementOther").click(function () {

        loadAchievement('other');
    });
    $(".loadAchievementNone").click(function () {

        loadAchievement('none');
    });

    
});

