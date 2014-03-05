// jppviewmodels.js
//
// Defines various viewmodels for JPP

// Generates a valid image path for a supplied src
// @param imgSrc Base url image can be found at
// @param size Optional desired size (s, m)
// @param profile Should a null src be replaced with the default profile image?
// @return Valid url for the given src and size
var cleanImageURL = function (imgSrc, size) {
    var imageSrc = '';

    if (imgSrc === null) {
        imageSrc = null;
    }
    else {
        // Replace back-slashes, and remove tilda from beginning of string
        imageSrc = imgSrc.replace(/\\/g, "/").substr(1);
        if (size === 's' || size === 'm') {
            imageSrc = imageSrc.replace(/\.([^.]+)$/, '_' + size + '.$1');
        }
    }

    return imageSrc;
}

// Determines if a value n is a number
// @param n Unitless value to check
// @return boolean true/false
// From: http://stackoverflow.com/questions/18082/validate-numbers-in-javascript-isnumeric/1830844#1830844
var isNumber = function (n) {
    return !isNaN(parseFloat(n)) && isFinite(n);
}

// Builds an object containing all information relating to an earning instance
// @param data JSON data representing the earning
function Earning(data, baseURL) {
    var self = this;

    // Earning Details
    self.earningID = data.EarningID;
    self.templateID = data.TemplateID;
    self.title = data.Title;
    self.earningIsAchievement = data.EarningIsAchievement;
    self.shareURL = baseURL + (self.earningIsAchievement ? '/Achievements/' : '/Quests/') + self.templateID + '#' + self.earningID;
    if (!self.earningIsAchievement) self.shareURL += "-1";

    // Player Details
    self.playerID = data.PlayerID;
    self.displayName = data.DisplayName;
    self.image = cleanImageURL(data.Image, 'm');
    self.playerImage = cleanImageURL(data.PlayerImage, null);
    if (self.playerImage === null) self.playerImage = '/Content/Images/Jpp/defaultProfileAvatar.png';
    self.earnedDate = new Date(parseInt(data.EarnedDate.substr(6))).toLocaleDateString();

    // Earning content
    self.contentPhoto = ko.observable(cleanImageURL(data.ContentPhoto, null));
    self.contentText = data.ContentText;
    self.contentURL = data.ContentURL;
    self.storyPhoto = ko.observable(cleanImageURL(data.StoryPhoto, null));
    self.storyText = ko.observable(data.StoryText);

    // Story
    self.manageStoryFocus = ko.observable(false);
    self.manageStoryVisible = ko.observable(false);
    self.manageStoryVisibleListener = ko.computed(function () {
        if (self.manageStoryFocus())
            self.manageStoryVisible(true);
    }, self);
    self.storyImageFile = ko.observable('');

    // Comments
    self.comments = ko.observableArray();
    if (data.Comments !== null)
    {
        for (var i = 0; i < data.Comments.length; i++) {
            self.comments.push(new Comment(data.Comments[i]));
        }
    }

    // States
    self.submitting = false;

    self.manageStoryFormVisible = ko.observable(true);
    self.manageStoryLoading = ko.observable(false);
    self.manageStoryError = ko.observable(false);

    // Permissions
    self.commentsDisabled = data.CommentsDisabled;
    self.currentUserCanAddStory = data.CurrentUserCanAddStory;
    self.currentUserCanEditStory = data.CurrentUserCanEditStory;

    // Clears the form and hides the buttons
    self.cancelManageStory = function (data, event) {
        $(event.currentTarget).parents('.manage-story-form')[0].reset();
        self.storyImageFile('');
        self.manageStoryVisible(false);
    }
    // Triggers the actual file input type click event
    self.fileInputClick = function (data, event) {
        $(event.currentTarget).siblings('.file-input').click();
    }
    self.updateFilePath = function (data, event) {
        // Insert text and strip dummy path
        self.storyImageFile(event.currentTarget.value.replace(/C:\\fakepath\\/i, ''));
    }

    self.saveStory = function (d, e) {
        e.preventDefault();
        self.manageStoryLoading(true);
        self.manageStoryFormVisible(false);

        var form = $(e.target).parents('form');

        form.ajaxSubmit({
            clearForm: true,
            success: function (responseObj) {
                self.manageStoryLoading(false);
                console.log("SUCCESS: Save Story");
            },
            error: function () {
                self.manageStoryLoading(false);
                self.manageStoryError(true);
            }
        });

        return true;
    }


    // Sends a request to add a comment and adds to array if successful
    self.addComment = function (d, e) {
        // Submit when enter key is pressed without shift key
        if (e.keyCode == 13) {

            // Submit if shift key isn't currently held
            if (!e.shiftKey && !self.submitting) {
                self.submitting = true;
                var form = $(e.target).parents('form');
                form.children('input[name=text]').prop('disabled', true);

                form.ajaxSubmit({
                    clearForm: true,
                    success: function (responseObj) {
                        // If comment was successfully added, add it in the view
                        self.comments.push(new Comment(responseObj));
                        form.children('input[name=text]').prop('disabled', false);

                        // Enable submissions
                        self.submitting = false;
                    },
                    error: function () {
                        //TODO: Highlight input field
                        // Enable submissions
                        self.submitting = false;
                        console.log("ERROR: Comment Submission");
                    }

                })
            }
            return false;
        }

        // Allow key inputs
        return true;
    }

}

// Builds an empty earning object
function EmptyEarning() {
    var self = this;

    // Earning Details
    self.earningID = 0;
    self.templateID = 0;
    self.title = "";
    self.earningIsAchievement = false;
    self.shareURL = "";
    self.image = "";

    // Player Details
    self.playerID = 0;
    self.displayName = "";
    self.playerImage = '/Content/Images/Jpp/defaultProfileAvatar.png';
    self.earnedDate = 0;

    // Earning content
    self.contentPhoto = null;
    self.contentText = "";
    self.contentURL = "";
    self.storyPhoto = null;
    self.storyText = ""

    // Comments
    self.comments = ko.observableArray();

    // States
    self.submitting = false;

    // Permissions
    self.commentsDisabled = true;
    self.currentUserCanAddStory = false;
    self.currentUserCanEditStory = false;

}

// Data and functions for a comment
// @param data Initial data to build a comment with
function Comment(data) {
    var self = this;
    self.commentID = data.ID;
    self.deleted = ko.observable(data.Deleted);
    self.playerID = ko.observable(data.PlayerID);
    self.playerDisplayName = ko.observable(data.DisplayName);
    self.playerImage = ko.observable(cleanImageURL(data.PlayerImage, null));
    if (self.playerImage() === null) self.playerImage('/Content/Images/Jpp/defaultProfileAvatar.png');
    self.text = ko.observable(data.Text);
    self.currentUserCanDelete = ko.observable(data.CurrentUserCanDelete);
    self.currentUserCanEdit = ko.observable(data.CurrentUserCanEdit);
    self.editing = ko.observable(false);
    self.commentDate = new Date(parseInt(data.CommentDate.substr(6))).toLocaleString();

    // Switches editing mode on call
    self.invertEditing = function () {
        self.editing(!self.editing());
        return true;
    }

    // Sends a request to delete a comment and removes comment data if successful
    self.deleteComment = function (d, e) {
        var form = $(e.target).parents('form');

        form.ajaxSubmit({
            // TODO: Clear text faster
            clearForm: true,
            success: function (responseObj) {
                // Remove comment on success
                self.deleted(responseObj.Deleted);
                self.playerID(responseObj.PlayerID);
                self.playerDisplayName(responseObj.DisplayName);
                self.playerImage(null);
                self.text(responseObj.Text);
                self.currentUserCanDelete(responseObj.CurrentUserCanDelete);
                self.currentUserCanEdit(responseObj.CurrentUserCanEdit);
            },
            error: function () {
                //TODO: alert user
                console.log("ERROR: Comment deletion");
            }
        })
    }

    // Sends a request to edit a comment and updates comment if successful
    self.editComment = function (d, e) {
        var form = $(e.target).parents('form');
        self.text(form.children('input[name=text]').val());
        self.invertEditing();

        form.ajaxSubmit({
            clearForm: false,
            success: function (responseObj) {
                // Remove comment on success
                self.text(responseObj.Text);
            },
            error: function () {
                //TODO: alert user
                console.log("ERROR: Comment editing");
            }
        })
    }
}

// ViewModel for the Earning List
// @param settings JSON list of options for data retrieval
function EarningListViewModel(settings) {
    var self = this;

    // Options
    self.loadInterval = 6;
    self.loadCount = 0;
    self.scrollBuffer = 400;
    self.playerID = settings.playerID;
    self.achievementID = settings.achievementID;
    self.questID = settings.questID;
    self.isLoading = ko.observable(false);
    self.atEnd = ko.observable(false);
    self.isEmpty = ko.observable(false);
    self.shareURLBase = location.protocol + '//' + location.host;
    
    // Dynamic data
    self.earnings = ko.observableArray();

    // Functions
    // Retrieves earning data from server and appends it to the earning array
    self.loadEarnings = function () {
        // Only show the loading spinner if there is probably more stuff to load
        if (self.loadCount % self.loadInterval == 0) {
            self.isLoading(true);
        }
        else {
            self.atEnd(true);
        }
        
        // Ajax request
        $.get("/JSON/Earnings", {
                id: self.playerID,
                achievementID: self.achievementID,
                questID: self.questID,
                start: self.loadCount,
                count: self.loadInterval
        }).done(function (data) {

            var dataCount = data.Earnings.length;

            // Update loadCount
            self.loadCount += dataCount;

            if (self.loadCount === 0) {
                self.isLoading(false);
                self.isEmpty(true);
                return;
            }

            // Build earnings
            for (var i = 0; i < dataCount; i++) {
                self.earnings.push(new Earning(data.Earnings[i], self.shareURLBase));
            }

            // Bind scroll and textarea autoresize
            if (dataCount > 0) {
                $(window).bind('scroll', self.bindScroll);
                $('.story-text').autosize();
            }
            else {
                self.atEnd(true);
            }

            self.isLoading(false);
        });
    };

    // Callback for scroll event to handle additional loading
    //TODO: Bind scroll to parent to support multiple lists
    self.bindScroll = function () {
        // Check for a scroll to the bottom of the timelineFeed - scrollBuffer
        if ($(window).scrollTop() + $(window).height() >= $('.earningFeed')[0].scrollHeight + $('.earningFeed').offset().top - self.scrollBuffer) {
            $(window).unbind('scroll');
            self.loadEarnings();
        }
    };

    // Initial load
    self.loadEarnings();
}

// ViewModel for fullscreen earning
function ShareEarningViewModel() {
    var self = this;

    // Earning object that will be displayed to user
    self.currentEarning = ko.observable(new EmptyEarning());

    self.scrolledHeight = ko.observable(0);

    self.fullscreenEarningVisible = ko.observable(false);
    self.loading = ko.observable(false);
    self.shareURLBase = location.protocol + '//' + location.host;

    self.loadEarning = function (eID, eIsA) {

        self.fullscreenEarningVisible(true);
        self.loading(true);

        // Ajax request
        $.get("/JSON/Earning", {
            id: eID,
            isAchievement: eIsA
        }).done(function (data) {

            // TODO: Handle errors
            // 0. Invalid earning ID for URL
            // 1. Invalid permissions
            
            // Save into currentEarning field to update view
            self.currentEarning(new Earning(data, self.shareURLBase));
            
            self.loading(false);
        });
    }

    // On click event for 'X' and grey area
    self.closeFullscreenEarning = function () {
        // Clear hash
        var scrollV, scrollH, loc = window.location;
        if ("pushState" in history)
            history.pushState("", document.title, loc.pathname + loc.search);
        else {
            // Prevent scrolling by storing the page's current scroll offset
            scrollV = document.body.scrollTop;
            scrollH = document.body.scrollLeft;

            loc.hash = "";

            // Restore the scroll offset, should be flicker free
            document.body.scrollTop = scrollV;
            document.body.scrollLeft = scrollH;
        }

        // Hide earning
        self.fullscreenEarningVisible(false);
    }

    // Window listener for hash changes
    self.bindHashChange = function () {
        // Listen for hash change
        $(window).on('hashchange', function () {

            if( !self.checkHash() )
                self.closeFullscreenEarning();
        });   
    }

    // Validates any change in window hash
    self.checkHash = function () {
        if (location.hash.length > 1) {
            var eIsAchievement = true;
            var hash = location.hash.substring(1);
            var values = hash.split('-');
            if (values.length < 1) return false;

            // On change, validate hash value is a number
            if (isNumber(values[0])) {
                // If a second value of 1 was passed, the earning is a quest
                if (values.length > 1 && isNumber(values[1]) && values[1] == 1) eIsAchievement = false;

                // Move view to current scrolled height
                var scrollAmount = $("body").scrollTop();
                self.scrolledHeight(scrollAmount);

                // Load new earning
                self.loadEarning(values[0], eIsAchievement);

                return true;
            }
        }
    }

    self.checkHash();
    self.bindHashChange();
}


// Containing object for achievement list items
function Achievement(data) {
    var self = this;
    self.ID = data.ID;
    self.image = cleanImageURL(data.Image, 'm');
    self.pointsCreate = data.PointsCreate;
    self.pointsExplore = data.PointsExplore;
    self.pointsLearn = data.PointsLearn;
    self.pointsSocialize = data.PointsSocialize;
    self.title = data.Title;
    self.tokenizedTitle = self.title.toLowerCase().match(/\S+/g);
    self.visible = ko.observable(true);
}

// View Model for the achievement list
// @param settings JSON list of options for data retrieval
function AchievementListViewModel(settings) {
    var self = this;


    //
    // Options
    //

    // Base component of the query string
    self.queryStringBase = '/JSON/Achievements';
    // Lists user may select to query new data
    self.lists = ['All', 'Earned', 'Locked'];
    // List toggles may be null, true, or false
    self.listToggle = null;
    // Every option for list order
    self.orderOptions = [{ name: 'A-Z', value: 'az' }, { name: 'Z-A', value: 'za' }];
    // Passed in id of the currently logged user
    self.playerID = settings.playerID;
    // The currently selected list
    self.activeList = ko.observable();
    // User-entered string to filter
    self.searchText = ko.observable('');
    // Boolean flag for loading state
    self.loading = ko.observable(false);
    // Boolean flag for empty state
    self.empty = ko.observable(false);



    //
    // Data
    //

    // All achievement data returned from query
    self.listItems = ko.observableArray();
    // Filtered achievement data to display
    self.displayListItems = ko.computed(function () {
        // Filter checkbox options
        var itemArray = ko.utils.arrayFilter(self.listItems(), function (item) {
            return self.checkboxFilter(item);
        });

        // Filter search text (tokenized)
        var filterArray = self.searchText().toLowerCase().match(/\S+/g);
        if (filterArray !== null && filterArray.length > 0) {
            // Filter search text
            itemArray = ko.utils.arrayFilter(itemArray, function (item) {

                // Get tokenized title
                var tokenArray = item.tokenizedTitle;

                // Loop through each token
                for (var i = 0; i < tokenArray.length; i++) {
                    
                    // If the search text is longer than the item title we don't have a match
                    if ((tokenArray.length - i) >= filterArray.length) {

                        var dummyCheck = true;

                        // The first j-1 tokens must match the first j-1 tokens starting at index i
                        for (var j = 0; j < filterArray.length-1; j++) {
                            if (filterArray[j] !== tokenArray[i + j]) {
                                dummyCheck = false;
                            }
                        }

                        // We only want to return true here, as a later token in the same title could match
                        if (dummyCheck && self.stringBeginsWith(filterArray[filterArray.length - 1], tokenArray[i + filterArray.length - 1]))
                            return true;
                    }
                        
                        
                }
                return false;

            });
        }

        //self.filterAlphabetical();

        // Display empty message if there are no items
        self.empty(itemArray.length <= 0);

        return itemArray;
    }, self);

    // Quad Filters
    self.createChecked = ko.observable(true);
    self.exploreChecked = ko.observable(true);
    self.learnChecked = ko.observable(true);
    self.socializeChecked = ko.observable(true);

    // Alphabetical Ordering
    /*
    self.order = ko.observable('az');
    self.order.subscribe(function (newData) {
        self.filterAlphabetical();
    }, self);
    */


    //
    // Functions
    //

    // Checks an item to determine if it should be shown
    // @param item The item to check
    // @returns true if item should be shown
    // @returns false if item should be removed
    self.checkboxFilter = function (item) {
        if (self.createChecked() && item.pointsCreate > 0) {
            return true;
        }
        else if (self.exploreChecked() && item.pointsExplore > 0) {
            return true;
        }
        else if (self.learnChecked() && item.pointsLearn > 0) {
            return true;
        }
        else if (self.socializeChecked() && item.pointsSocialize > 0) {
            return true;
        }
        else {
            return false;
        }
    }

    // Retrieves achievement data from server and appends it to the earning array
    // TODO: Load 28 and then the rest to speed up load
    self.loadItems = function () {

        // Clear current list
        self.listItems.removeAll();

        // Show loading spinner
        self.loading(true);

        // Hide empty message
        self.empty(false);
        
        // Ajax request
        $.get(self.queryStringBase, {
            userID: self.playerID,
            //start: 0,
            //count: 6,
            achievementsEarned: self.listToggle
        }).done(function (data) {

            var dataCount = data.Achievements.length;

            // Build new achievements
            for (var i = 0; i < dataCount; i++) {
                self.listItems.push(new Achievement(data.Achievements[i]));
            }

            // Ensure alphabetical ordering
            self.filterAtoZ();

            // Empty message
            if (dataCount == 0) {
                self.empty(true);
            }

            // Hide loading icon
            self.loading(false);
        });
    };

    // Load a specific list
    // @param list String title of list to load
    self.loadList = function (list) {
        if (list !== self.activeList()) {

            self.activeList(list);
            switch (list) {
                case self.lists[0]:
                    self.listToggle = null;
                    break;
                case self.lists[1]:
                    self.listToggle = true;
                    break;
                case self.lists[2]:
                    self.listToggle = false;
                    break;
                default:
                    self.listToggle = null;
                    break;
            }

            self.loadItems();
        }
    }

    // Filters items based on selected ordering
    self.filterAlphabetical = function () {
        if (self.order() === 'az') self.filterAtoZ();
        else self.filterZtoA();
    }

    // Filters items A to Z
    self.filterAtoZ = function () {
        self.listItems.sort(function (left, right) { return left.title == right.title ? 0 : (left.title < right.title ? -1 : 1) });
    }

    // Filters items Z to A
    self.filterZtoA = function () {
        self.listItems.sort(function (left, right) { return left.title == right.title ? 0 : (right.title < left.title ? -1 : 1) });
    }

    // Checks a string to see if it begins with another
    self.stringBeginsWith = function (needle, haystack) {
        return (haystack.substr(0, needle.length) == needle);
    }

    // Initial load
    self.loadList('All');
}



// Containing object for quest list items
function Quest(data) {
    var self = this;
    self.ID = data.ID;
    self.image = cleanImageURL(data.Image, 'm');
    self.title = data.Title;
    self.tokenizedTitle = self.title.toLowerCase().match(/\S+/g);
}

// View Model for the quest list
// @param settings JSON list of options for data retrieval
function QuestListViewModel(settings) {
    var self = this;


    //
    // Options
    //

    // Base component of the query string
    self.queryStringBase = '/JSON/Quests';
    // Lists user may select to query new data
    self.lists = ['All', 'Earned', 'Locked'];
    // List toggles may be null, true, or false
    self.listToggle = null;
    // Every option for list order
    self.orderOptions = [{ name: 'A-Z', value: 'az' }, { name: 'Z-A', value: 'za' }];
    // Passed in id of the currently logged user
    self.playerID = settings.playerID;
    // The currently selected list
    self.activeList = ko.observable();
    // User-entered string to filter
    self.searchText = ko.observable('');
    // Boolean flag for loading state
    self.loading = ko.observable(false);
    // Boolean flag for empty state
    self.empty = ko.observable(false);



    //
    // Data
    //

    // All achievement data returned from query
    self.listItems = ko.observableArray();
    // Filtered achievement data to display
    self.displayListItems = ko.computed(function () {
        // Filter checkbox options
        var itemArray = ko.utils.arrayFilter(self.listItems(), function (item) {
            return self.checkboxFilter(item);
        });

        // Filter search text (tokenized)
        var filterArray = self.searchText().toLowerCase().match(/\S+/g);
        if (filterArray !== null && filterArray.length > 0) {
            // Filter search text
            itemArray = ko.utils.arrayFilter(itemArray, function (item) {

                // Get tokenized title
                var tokenArray = item.tokenizedTitle;

                // Loop through each token
                for (var i = 0; i < tokenArray.length; i++) {

                    // If the search text is longer than the item title we don't have a match
                    if ((tokenArray.length - i) >= filterArray.length) {

                        var dummyCheck = true;

                        // The first j-1 tokens must match the first j-1 tokens starting at index i
                        for (var j = 0; j < filterArray.length - 1; j++) {
                            if (filterArray[j] !== tokenArray[i + j]) {
                                dummyCheck = false;
                            }
                        }

                        // We only want to return true here, as a later token in the same title could match
                        if (dummyCheck && self.stringBeginsWith(filterArray[filterArray.length - 1], tokenArray[i + filterArray.length - 1]))
                            return true;
                    }


                }
                return false;

            });
        }

        // Display empty message if there are no items
        self.empty(itemArray.length <= 0);

        return itemArray;
    }, self);

    // Quad Filters
    self.systemChecked = ko.observable(true);
    self.communityChecked = ko.observable(true);

    // Alphabetical Ordering
    self.order = ko.observable('az');
    self.order.subscribe(function (newData) {
        self.filterAlphabetical();
    }, self);



    //
    // Functions
    //

    // Checks an item to determine if it should be shown
    // @param item The item to check
    // @returns true if item should be shown
    // @returns false if item should be removed
    self.checkboxFilter = function (item) {
        /*
        if (self.systemChecked() && item.system) {
            return true;
        }
        else if (self.communityChecked() && item.community) {
            return true;
        }
        else {
            return false;
        }
        */

        return true;
    }

    // Retrieves achievement data from server and appends it to the earning array
    // TODO: Load 28 and then the rest to speed up load
    self.loadItems = function () {

        // Clear current list
        self.listItems.removeAll();

        // Show loading spinner
        self.loading(true);

        // Hide empty message
        self.empty(false);

        // Ajax request
        $.get(self.queryStringBase, {
            userID: self.playerID,
            //start: 0,
            //count: 6,
            questsEarned: self.listToggle
        }).done(function (data) {

            var dataCount = data.Quests.length;

            // Build new achievements
            for (var i = 0; i < dataCount; i++) {
                self.listItems.push(new Quest(data.Quests[i]));
            }

            // Ensure alphabetical ordering
            self.filterAlphabetical();

            // Empty message
            if (dataCount == 0) {
                self.empty(true);
            }

            // Hide loading icon
            self.loading(false);
        });
    };

    // Load a specific list
    // @param list String title of list to load
    self.loadList = function (list) {
        if (list !== self.activeList()) {

            self.activeList(list);
            switch (list) {
                case self.lists[0]:
                    self.listToggle = null;
                    break;
                case self.lists[1]:
                    self.listToggle = true;
                    break;
                case self.lists[2]:
                    self.listToggle = false;
                    break;
                default:
                    self.listToggle = null;
                    break;
            }

            self.loadItems();
        }
    }

    // Filters items based on selected ordering
    self.filterAlphabetical = function () {
        if (self.order() === 'az') self.filterAtoZ();
        else self.filterZtoA();
    }

    // Filters items A to Z
    self.filterAtoZ = function () {
        self.listItems.sort(function (left, right) { return left.title == right.title ? 0 : (left.title < right.title ? -1 : 1) });
    }

    // Filters items Z to A
    self.filterZtoA = function () {
        self.listItems.sort(function (left, right) { return left.title == right.title ? 0 : (right.title < left.title ? -1 : 1) });
    }

    // Checks a string to see if it begins with another
    self.stringBeginsWith = function (needle, haystack) {
        return (haystack.substr(0, needle.length) == needle);
    }

    // Initial load
    self.loadList('All');
}



// Containing object for player list items
function Player(data) {
    var self = this;
    self.ID = data.ID;
    self.displayName = data.DisplayName;
    self.firstName = data.FirstName;
    self.middleName = data.MiddleName;
    self.lastName = data.LastName;
    self.image = cleanImageURL(data.Image, null);
    if (self.image === null) self.image = '/Content/Images/Jpp/defaultProfileAvatar.png';
}

// View Model for the player list
// @param settings JSON list of options for data retrieval
function PlayerListViewModel(settings) {
    var self = this;


    //
    // Options
    //

    // Base component of the query string
    self.queryStringBase = '/JSON/Players';
    // Lists user may select to query new data
    self.lists = ['All', 'Friends', 'Non-Friends'];
    // List toggles may be null, true, or false
    self.listToggle = null;
    // Every option for list order
    self.orderOptions = [{ name: 'A-Z', value: 'az' }, { name: 'Z-A', value: 'za' }];
    // Passed in id of the currently logged user
    self.playerID = settings.playerID;
    // The currently selected list
    self.activeList = ko.observable();
    // User-entered string to filter
    self.searchText = ko.observable('');
    // Boolean flag for loading state
    self.loading = ko.observable(false);
    // Boolean flag for empty state
    self.empty = ko.observable(false);



    //
    // Data
    //

    // All data returned from query
    self.listItems = ko.observableArray();
    // Filtered data to display
    self.displayListItems = ko.computed(function () {
        // Filter checkbox options
        var itemArray = ko.utils.arrayFilter(self.listItems(), function (item) {
            return self.checkboxFilter(item);
        });

        // Filter search text
        var filter = self.searchText().toLowerCase();
        if (filter) {
            // Filter search text
            itemArray = ko.utils.arrayFilter(itemArray, function (item) {
                return  self.stringBeginsWith(filter, item.displayName.toLowerCase()) ||
                        self.stringBeginsWith(filter, item.firstName.toLowerCase()) ||
                        self.stringBeginsWith(filter, item.lastName.toLowerCase());
            });
        }

        // Display empty message if there are no items
        self.empty(itemArray.length <= 0);

        return itemArray;
    }, self);

    // Live Filters
    self.systemChecked = ko.observable(true);
    self.communityChecked = ko.observable(true);

    // Alphabetical Ordering
    self.order = ko.observable('az');
    self.order.subscribe(function (newData) {
        self.filterAlphabetical();
    }, self);



    //
    // Functions
    //

    // Checks an item to determine if it should be shown
    // @param item The item to check
    // @returns true if item should be shown
    // @returns false if item should be removed
    self.checkboxFilter = function (item) {
        /*
        if (self.systemChecked() && item.system) {
            return true;
        }
        else if (self.communityChecked() && item.community) {
            return true;
        }
        else {
            return false;
        }
        */

        return true;
    }

    // Retrieves achievement data from server and appends it to the earning array
    // TODO: Load 28 and then the rest to speed up load
    self.loadItems = function () {

        // Clear current list
        self.listItems.removeAll();

        // Show loading spinner
        self.loading(true);

        // Hide empty message
        self.empty(false);

        // Ajax request
        $.get(self.queryStringBase, {
            userID: self.playerID,
            //start: 0,
            //count: 6,
            friendsWith: self.listToggle
        }).done(function (data) {

            var dataCount = data.People.length;

            // Build new achievements
            for (var i = 0; i < dataCount; i++) {
                self.listItems.push(new Player(data.People[i]));
            }

            // Ensure alphabetical ordering
            self.filterAlphabetical();

            // Empty message
            if (dataCount == 0) {
                self.empty(true);
            }

            // Hide loading icon
            self.loading(false);
        });
    };

    // Load a specific list
    // @param list String title of list to load
    self.loadList = function (list) {
        if (list !== self.activeList()) {

            self.activeList(list);
            switch (list) {
                case self.lists[0]:
                    self.listToggle = null;
                    break;
                case self.lists[1]:
                    self.listToggle = true;
                    break;
                case self.lists[2]:
                    self.listToggle = false;
                    break;
                default:
                    self.listToggle = null;
                    break;
            }

            self.loadItems();
        }
    }

    // Filters items based on selected ordering
    self.filterAlphabetical = function () {
        if (self.order() === 'az') self.filterAtoZ();
        else self.filterZtoA();
    }

    // Filters items A to Z
    self.filterAtoZ = function () {
        self.listItems.sort(function (left, right) { return left.displayName == right.displayName ? 0 : (left.displayName < right.displayName ? -1 : 1) });
    }

    // Filters items Z to A
    self.filterZtoA = function () {
        self.listItems.sort(function (left, right) { return left.displayName == right.displayName ? 0 : (right.displayName < left.displayName ? -1 : 1) });
    }

    // Checks a string to see if it begins with another
    self.stringBeginsWith = function (needle, haystack) {
        return (haystack.substr(0, needle.length) == needle);
    }

    // Initial load
    self.loadList('Friends');
}


function ProfileQuestListViewModel(settings) {
    var self = this;

    self.playerID = settings.playerID;
    // Base component of the query string
    self.queryStringBase = '/JSON/Quests';
    self.loadInterval = 28;
    self.loadCount = 0;
    self.loading = ko.observable(false);
    self.empty = ko.observable(false);
    self.listItems = ko.observableArray();
    self.total = ko.observable(0);

    // Retrieves achievement data from server and appends it to the earning array
    // TODO: Load 28 and then the rest to speed up load
    self.loadItems = function () {

        // Show loading spinner
        self.loading(true);

        // Hide empty message
        self.empty(false);

        // Ajax request
        $.get(self.queryStringBase, {
            userID: self.playerID,
            start: 0,
            count: self.loadInterval,
            questsEarned: true
        }).done(function (data) {

            var dataCount = data.Quests.length;
            self.total(data.Total);

            // Build new achievements
            for (var i = 0; i < dataCount; i++) {
                self.listItems.push(new Quest(data.Quests[i]));
            }

            // Ensure alphabetical ordering
            self.filterAtoZ();

            // Empty message
            if (dataCount == 0) {
                self.empty(true);
            }

            // Hide loading icon
            self.loading(false);
        });
    };

    // Filters items A to Z
    self.filterAtoZ = function () {
        self.listItems.sort(function (left, right) { return left.title == right.title ? 0 : (left.title < right.title ? -1 : 1) });
    }

    self.loadItems();
}

function ProfileAchievementListViewModel(settings) {
    var self = this;

    self.playerID = settings.playerID;
    // Base component of the query string
    self.queryStringBase = '/JSON/Achievements';
    self.loadInterval = 28;
    self.loadCount = 0;
    self.loading = ko.observable(false);
    self.empty = ko.observable(false);
    self.listItems = ko.observableArray();
    self.total = ko.observable(0);

    // Retrieves achievement data from server and appends it to the earning array
    // TODO: Load 28 and then the rest to speed up load
    self.loadItems = function () {

        // Show loading spinner
        self.loading(true);

        // Hide empty message
        self.empty(false);

        // Ajax request
        $.get(self.queryStringBase, {
            userID: self.playerID,
            start: 0,
            count: self.loadInterval,
            achievementsEarned: true
        }).done(function (data) {
            //http://jpp-rit-sandbox.azurewebsites.net/JSON/Achievements?userID=1&achievementsEarned=true
            //http://localhost:5376/JSON/Achievements?achievementsEarned=true

            var dataCount = data.Achievements.length;
            self.total(data.Total);

            // Build new achievements
            for (var i = 0; i < dataCount; i++) {
                self.listItems.push(new Achievement(data.Achievements[i]));
            }

            // Ensure alphabetical ordering
            self.filterAtoZ();

            // Empty message
            if (dataCount == 0) {
                self.empty(true);
            }

            // Hide loading icon
            self.loading(false);
        });
    };

    // Filters items A to Z
    self.filterAtoZ = function () {
        self.listItems.sort(function (left, right) { return left.title == right.title ? 0 : (left.title < right.title ? -1 : 1) });
    }

    self.loadItems();
}