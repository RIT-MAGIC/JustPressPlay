using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.ComponentModel.DataAnnotations;
using WebMatrix.WebData;

using JustPressPlay.Utilities;
using JustPressPlay.Models;
using JustPressPlay.Models.Repositories;

namespace JustPressPlay.Controllers
{
	/// <summary>
	/// Self-contained controller for Importing Data - This may just
	/// be temporary as we only need it to get data from RIT's V2 to V3
	/// </summary>
	public class ImportDataController : Controller
	{
		#region View Model & Sub Classes
		/// <summary>
		/// Contains the DB files to import
		/// </summary>
		public class ImportDataViewModel
		{
			[Required]
			[StringLength(1)]
			public String Delimiter { get; set; }

			[Required]
			public HttpPostedFileBase UserTable { get; set; }

			[Required]
			public HttpPostedFileBase ProfileTable { get; set; }

			//[Required]
			public HttpPostedFileBase FriendTable { get; set; }

			[Required]
			public HttpPostedFileBase AchievementTemplateTable { get; set; }

			[Required]
			public HttpPostedFileBase AchievementPointTemplateTable { get; set; }

			[Required]
			public HttpPostedFileBase AchievementRequirementsTable { get; set; }

			[Required]
			public HttpPostedFileBase AchievementCaretakerTable { get; set; }

			[Required]
			public HttpPostedFileBase AchievementUserStoryTable { get; set; }

			[Required]
			public HttpPostedFileBase AchievementUserContentTable { get; set; }

			[Required]
			public HttpPostedFileBase AchievementInstanceTable { get; set; }

			[Required]
			public HttpPostedFileBase AchievementPointInstanceTable { get; set; }

			[Required]
			public HttpPostedFileBase QuestTemplateTable { get; set; }

			[Required]
			public HttpPostedFileBase QuestAchievementStepTable { get; set; }

			[Required]
			public HttpPostedFileBase QuestInstanceTable { get; set; }
		}

		/// <summary>
		/// Contains information about an imported user
		/// </summary>
		public class ImportedUser
		{
			public String Username { get; set; }
			public String Email { get; set; }
			public bool UsernameConflict { get; set; }
			public bool EmailConflict { get; set; }
			public int OldID { get; set; }
			public int NewID { get; set; }
			public bool UserWasDeleted { get; set; }
		}

		/// <summary>
		/// Contains info about an imported achievement or quest
		/// </summary>
		public class ImportedEarnable
		{
			public String UniqueData { get; set; }
			public int OldID { get; set; }
			public int NewID { get; set; }
		}
		#endregion

		#region Private Variables
		// User information, stored with the OLD ID as the key
		private Dictionary<int, ImportedUser> _userMap;
		private Dictionary<int, ImportedEarnable> _achievementMap;
		private Dictionary<int, ImportedEarnable> _questMap;
		private Dictionary<int, ImportedEarnable> _userStoryMap;
		private Dictionary<int, ImportedEarnable> _userContentMap;
		#endregion

		#region Actions
		/// <summary>
		/// Allows a full admin to import old JPPv2 data
		/// </summary>
		/// <returns>GET: /Admin/ImportData</returns>
		[Authorize(Roles = JPPConstants.Roles.FullAdmin)]
		public ActionResult Index()
		{
			return View();
		}

		/// <summary>
		/// Allows a full admin to import old JPPv2 data
		/// </summary>
		/// <param name="model">The files to import</param>
		/// <returns>GET: /Admin/ImportData</returns>
		[HttpPost]
		[Authorize(Roles = JPPConstants.Roles.FullAdmin)]
		public ActionResult Index(ImportDataViewModel model)
		{
			if (ModelState.IsValid)
			{
				UnitOfWork work = new UnitOfWork();
				_userMap = new Dictionary<int, ImportedUser>();
				_achievementMap = new Dictionary<int, ImportedEarnable>();
				_questMap = new Dictionary<int, ImportedEarnable>();
				_userStoryMap = new Dictionary<int, ImportedEarnable>();
				_userContentMap = new Dictionary<int, ImportedEarnable>();

				//try
				{
					// Do the importing
					ImportUsers(model.UserTable, model.Delimiter, work);
					ImportProfiles(model.ProfileTable, model.Delimiter, work);
					ImportFriends(model.FriendTable, model.Delimiter, work);
					ImportAchievements(
						model.AchievementTemplateTable,
						model.AchievementPointTemplateTable,
						model.AchievementRequirementsTable,
						model.AchievementCaretakerTable,
						model.Delimiter,
						work);
					ImportAchievementUserStuff(
						model.AchievementUserStoryTable,
						model.AchievementUserContentTable,
						model.Delimiter,
						work);
					ImportAchievementInstances(
						model.AchievementInstanceTable,
						model.AchievementPointInstanceTable,
						model.Delimiter,
						work);
					ImportQuestTemplates(
						model.QuestTemplateTable,
						model.QuestAchievementStepTable,
						model.Delimiter,
						work);
					ImportQuestInstances(model.QuestInstanceTable, model.Delimiter, work);
				}
				//catch(Exception e)
				{
					//System.Diagnostics.Debug.WriteLine("Error: " + e.Message + "\n" + e.StackTrace);
					//return View(model);
				}

				// Should be good
				return RedirectToAction("Index");
			}

			return View(model);
		}
		#endregion

		#region Helpers
		/// <summary>
		/// Turns the posted file into a list of dictionaries
		/// </summary>
		/// <param name="file">The file w/ data (first row must be column names)</param>
		/// <param name="delimiter">Delimiter between data</param>
		/// <returns>List of dicts, or null</returns>
		private List<Dictionary<String, String>> GetDataFromFile(HttpPostedFileBase file, String delimiter)
		{
			if (file == null || file.ContentLength == 0)
			{
				System.Diagnostics.Debug.WriteLine("File null or length zero:" + file.ToString());
				return null;
			}

			List<Dictionary<String, String>> data = new List<Dictionary<string, string>>();

			try
			{
				// Read the file stream
				using (StreamReader reader = new StreamReader(file.InputStream))
				{
					// Keys and a dictionary for easier lookup
					String[] keys = reader.ReadLine().Split('|');

					// Loop through lines and grab data
					String line = "";
					while ((line = reader.ReadLine()) != null)
					{
						// Put this line into the data map
						Dictionary<String, String> dataRow = new Dictionary<string, string>();
						String[] values = line.Split(new string[] { delimiter }, StringSplitOptions.None);
						for (int i = 0; i < keys.Length; i++)
							dataRow.Add(keys[i], i < values.Length ? values[i] : "");
						data.Add(dataRow);
					}
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message + "\n" + e.StackTrace);
				return null;
			}

			return data;
		}

		/// <summary>
		/// Gets an imported user by their "old" ID
		/// </summary>
		/// <param name="oldID"></param>
		/// <returns></returns>
		private ImportedUser GetImportedUserByOldID(String oldID)
		{
			// Try to parse out id
			int id = -1;
			if (int.TryParse(oldID, out id) && _userMap.ContainsKey(id))
				return _userMap[id];

			return null;
		}

		/// <summary>
		/// Gets an imported earnable by its "old" ID
		/// </summary>
		/// <param name="dict"></param>
		/// <param name="oldID"></param>
		/// <returns></returns>
		private ImportedEarnable GetImportedEarnableByOldID(Dictionary<int, ImportedEarnable> dict, String oldID)
		{
			// Try to parse out id
			int id = -1;
			if (int.TryParse(oldID, out id) && dict.ContainsKey(id))
				return dict[id];

			return null;
		}
		#endregion

		#region Users
		/// <summary>
		/// Imports the user table from V2
		/// </summary>
		/// <param name="profileTable">The file w/ user table data</param>
		/// <param name="delimiter">The delimiter between data</param>
		/// <param name="work">The DB access</param>
		private void ImportUsers(HttpPostedFileBase userTable, String delimiter, UnitOfWork work)
		{
			// Grab the data
			List<Dictionary<String, String>> data = GetDataFromFile(userTable, delimiter);
			if (data == null)
			{
				ModelState.AddModelError("", "Error with User table.  Check Debug Output");
				return;
			}

			// Go through each data row and create users
			foreach (Dictionary<String, String> row in data)
			{
				String username = row["username"];
				String email = row["email"];

				// Make the user (don't add directly to DB!)
				object userObj = new
				{
					username = username,
					first_name = row["real_first_name"],
					middle_name = row["real_middle_name"],
					last_name = row["real_last_name"],
					is_player = Boolean.Parse(row["is_player"]),
					created_date = DateTime.Parse(row["date_account_created"]),
					status = Boolean.Parse(row["account_suspended"]) ? (int)JPPConstants.UserStatus.Suspended : (int)JPPConstants.UserStatus.Active,
					first_login = Boolean.Parse(row["account_first_login"]),
					email = email,
					last_login_date = DateTime.Parse(row["date_last_login"]),
					has_agreed_to_tos = false, // False for everyone!
					display_name = row["real_first_name"] + " " + row["real_last_name"],
					privacy_settings = (int)JPPConstants.PrivacySettings.FriendsOnly,
				};

				ImportedUser impUser = new ImportedUser()
				{
					Email = email,
					OldID = int.Parse(row["userID"]),
					Username = username,
					UserWasDeleted = false,
					UsernameConflict = false,
					EmailConflict = false
				};

				// Check for conflicts
				impUser.UsernameConflict = work.EntityContext.user.Where(u => u.username == username).Any();
				impUser.EmailConflict = work.EntityContext.user.Where(u => u.email == email).Any();
				if (!impUser.EmailConflict && !impUser.UsernameConflict)
				{
					// No conflicts, so add
					WebSecurity.CreateUserAndAccount(
						username,
						Guid.NewGuid().ToString(), // "Random" password - user will need to reset
						userObj,
						false); // No confirmation
				}

				// Either way, put in our local map, with the old ID as the key
				_userMap.Add(impUser.OldID, impUser);
			}

			work.SaveChanges();

			// Update all the imported users with new ids
			foreach (ImportedUser user in _userMap.Values)
			{
				// Look up via username
				int? newID = (from u in work.EntityContext.user where u.username == user.Username select u.id).FirstOrDefault();
				if (newID != null && newID.Value != 0)
				{
					user.NewID = newID.Value;
				}
				else
				{
					// Username not found, so try email.  If not email, we just can't find the user
					newID = (from u in work.EntityContext.user where u.email == user.Email select u.id).FirstOrDefault();
					if (newID != null && newID.Value != 0)
						user.NewID = newID.Value;
				}
			}

		}

		/// <summary>
		/// Imports the profile table from V2
		/// </summary>
		/// <param name="profileTable">The file w/ profile table data</param>
		/// <param name="delimiter">The delimiter between data</param>
		/// <param name="work">The DB access</param>
		private void ImportProfiles(HttpPostedFileBase profileTable, String delimiter, UnitOfWork work)
		{
			// Grab the data
			List<Dictionary<String, String>> data = GetDataFromFile(profileTable, delimiter);
			if (data == null)
			{
				ModelState.AddModelError("", "Error with Profile table.  Check Debug Output");
				return;
			}

			// Go through each data row and create users
			foreach (Dictionary<String, String> row in data)
			{
				int oldID = -1;
				if (!int.TryParse(row["userID"], out oldID) || !_userMap.ContainsKey(oldID))
					continue;

				// Get the user, skip if not found
				ImportedUser impUser = _userMap[oldID];
				user u = work.EntityContext.user.Find(impUser.NewID);
				if (u == null)
					continue;

				// Grab data from profile table
				u.display_name = row["display_name"];
				u.six_word_bio = row["six_word_bio"];
				u.full_bio = row["full_bio"];
				u.image = row["image"];
				u.personal_url = row["personalURL"];
				switch (int.Parse(row["privacy"]))
				{
					case 1: u.privacy_settings = (int)JPPConstants.PrivacySettings.FriendsOnly; break;			// Friends only (old value)
					case 2: u.privacy_settings = (int)JPPConstants.PrivacySettings.JustPressPlayOnly; break;	// JPP Only (old value)
					case 3: u.privacy_settings = (int)JPPConstants.PrivacySettings.Public; break;				// Public (old value)
				}
				// Only update if not currently suspended
				if (u.status != (int)JPPConstants.UserStatus.Suspended)
				{
					switch (int.Parse(row["left_game"]))
					{
						case 1: u.status = (int)JPPConstants.UserStatus.Active; break; // Active (old value)
						case 2: u.status = (int)JPPConstants.UserStatus.Deactivated; break; // Left Game (old value)
						case 3: u.status = (int)JPPConstants.UserStatus.Deleted; impUser.UserWasDeleted = true; break; // Deleted (old value)
					}
				}

				// TODO: Communication settings?
				// Old column name: "communications"
				// 1: Everything (old value)
				// 2: Minimal (old value)
			}

			work.SaveChanges();
		}

		/// <summary>
		/// Imports the friends table from V2
		/// </summary>
		/// <param name="friendsTable">File w/ friend data</param>
		/// <param name="delimiter">Delimiter between data</param>
		/// <param name="work">The DB access</param>
		private void ImportFriends(HttpPostedFileBase friendsTable, String delimiter, UnitOfWork work)
		{
			if (friendsTable == null)
				return;

			// Grab the data
			List<Dictionary<String, String>> data = GetDataFromFile(friendsTable, delimiter);
			if (data == null)
			{
				ModelState.AddModelError("", "Error with Friends table.  Check Debug Output");
				return;
			}

			// Go through each data row
			foreach (Dictionary<String, String> row in data)
			{
				// Do we know about this user?
				ImportedUser user1 = GetImportedUserByOldID(row["src_userID"]);
				ImportedUser user2 = GetImportedUserByOldID(row["dst_userID"]);
				if (user1 == null || user2 == null || user1.NewID == 0 || user2.NewID == 0)
					continue;

				// Friendship is bi-directional, so make 2
				friend friend1 = new friend()
				{
					source_id = user1.NewID,
					destination_id = user2.NewID,
					friended_date = DateTime.Parse(row["date_friended"]),
					request_date = DateTime.Parse(row["date_requested"])
				};
				friend friend2 = new friend()
				{
					source_id = friend1.destination_id,
					destination_id = friend1.source_id,
					friended_date = friend1.friended_date,
					request_date = friend1.request_date
				};

				work.EntityContext.friend.Add(friend1);
				work.EntityContext.friend.Add(friend2);
			}

			work.SaveChanges();
		}
		#endregion

		#region Achievements
		/// <summary>
		/// Imports the achievements from V2
		/// </summary>
		/// <param name="achieveTemplateTable">File w/ achievement template data</param>
		/// <param name="achievePointTemplateTable">Achievement point template data (to be rolled into achievement)</param>
		/// <param name="achieveRequirementsTable">Achievement requirement data</param>
		/// <param name="achieveCaretakerTable">Achievement caretaker data</param>
		/// <param name="delimiter">Delimiter between data</param>
		/// <param name="work">The DB access</param>
		private void ImportAchievements(
			HttpPostedFileBase achieveTemplateTable,
			HttpPostedFileBase achievePointTemplateTable,
			HttpPostedFileBase achieveRequirementsTable,
			HttpPostedFileBase achieveCaretakerTable,
			String delimiter,
			UnitOfWork work)
		{
			// Grab the data
			List<Dictionary<String, String>> achieveData = GetDataFromFile(achieveTemplateTable, delimiter);
			if (achieveData == null)
			{
				ModelState.AddModelError("", "Error with Achievement Template table.  Check Debug Output");
				return;
			}

			List<Dictionary<String, String>> pointData = GetDataFromFile(achievePointTemplateTable, delimiter);
			if (pointData == null)
			{
				ModelState.AddModelError("", "Error with Achievement Point Template table.  Check Debug Output");
				return;
			}

			List<Dictionary<String, String>> reqData = GetDataFromFile(achieveRequirementsTable, delimiter);
			if (reqData == null)
			{
				ModelState.AddModelError("", "Error with Achievement Requirements table.  Check Debug Output");
				return;
			}

			List<Dictionary<String, String>> caretakerData = GetDataFromFile(achieveCaretakerTable, delimiter);
			if (caretakerData == null)
			{
				ModelState.AddModelError("", "Error with Achievement Caretaker table.  Check Debug Output");
				return;
			}

			// The templates that need to be added
			List<achievement_template> templatesToAdd = new List<achievement_template>();

			// Go through each data row
			foreach (Dictionary<String, String> row in achieveData)
			{
				// Get related users
				ImportedUser editedBy = _userMap[int.Parse(row["last_modified_by"])];
				ImportedUser creator = _userMap[int.Parse(row["creatorID"])];
				if (creator == null)
					continue; // Must have a creator

				// Grab other info
				int oldID = int.Parse(row["achievementID"]);

				int state = (int)JPPConstants.AchievementQuestStates.Active;
				bool isRetired = Boolean.Parse(row["is_retired"]);
				if (isRetired)
					state = (int)JPPConstants.AchievementQuestStates.Retired;

				int type = (int)JPPConstants.AchievementTypes.Scan;
				switch (int.Parse(row["type"]))
				{
					// 1 - usersubmission
					case 1: type = (int)JPPConstants.AchievementTypes.UserSubmission; break;

					// 2 - scan
					case 2: type = (int)JPPConstants.AchievementTypes.Scan; break;

					// 3 - system
					case 3: type = (int)JPPConstants.AchievementTypes.System; break;

					// 4 - adminassigned
					case 4: type = (int)JPPConstants.AchievementTypes.AdminAssigned; break;

					// 5 - threshold
					case 5: type = (int)JPPConstants.AchievementTypes.Threshold; break;
				}

				int? triggerType = null;
				if (!String.IsNullOrWhiteSpace(row["system_trigger_type"]))
				{
					switch (int.Parse(row["system_trigger_type"]))
					{
						// TODO: Finalize this
						default: break;
					}
				}

				// Set up the template
				achievement_template template = new achievement_template()
				{
					id = oldID, // This will get overridden, but necessary for a look-up later
					content_type = String.IsNullOrWhiteSpace(row["content_type"]) ? (int?)null : int.Parse(row["content_type"]),
					created_date = DateTime.Parse(row["date_created"]),
					creator_id = creator.NewID,
					description = row["description"],
					featured = Boolean.Parse(row["is_featured"]),
					hidden = Boolean.Parse(row["is_hidden"]),
					icon = row["icon"],
					is_repeatable = Boolean.Parse(row["is_repeatable"]),
					last_modified_by_id = editedBy == null ? (int?)null : editedBy.NewID,
					modified_date = DateTime.Parse(row["date_modified"]),
					parent_id = String.IsNullOrWhiteSpace(row["parentID"]) ? (int?)null : int.Parse(row["parentID"]),
					posted_date = DateTime.Now,
					repeat_delay_days = String.IsNullOrWhiteSpace(row["repeat_delay"]) ? (int?)null : int.Parse(row["repeat_delay"]),
					retire_date = isRetired ? DateTime.Parse(row["date_retired"]) : (DateTime?)null,
					state = state,
					system_trigger_type = triggerType,
					threshold = String.IsNullOrWhiteSpace(row["threshhold"]) ? (int?)null : int.Parse(row["threshhold"]),
					title = row["title"],
					type = type,
				};

				// Create imported achievement
				ImportedEarnable impAchieve = new ImportedEarnable()
				{
					UniqueData = template.title,
					OldID = oldID
				};

				// Add to temporary list
				templatesToAdd.Add(template);
				_achievementMap.Add(impAchieve.OldID, impAchieve);
			}

			// Loop through points and put in correct achievements
			foreach (Dictionary<String, String> row in pointData)
			{
				int achievementID = int.Parse(row["achievementID"]);
				int categoryID = int.Parse(row["categoryID"]);
				int points = int.Parse(row["points"]);
				achievement_template template = templatesToAdd.Where(t => t.id == achievementID).FirstOrDefault();
				if (template == null)
					continue;

				// Switch on old category id
				switch (categoryID)
				{
					case 1: template.points_create = points; break;		// Create
					case 2: template.points_learn = points; break;		// Learn
					case 3: template.points_socialize = points; break;	// Socialize
					case 4: template.points_explore = points; break;	// Explore
				}
			}

			// Add templates to database
			foreach (achievement_template template in templatesToAdd)
				work.EntityContext.achievement_template.Add(template);
			work.SaveChanges();

			// Update the map with new ids
			foreach (ImportedEarnable e in _achievementMap.Values)
			{
				e.NewID = (from a in work.EntityContext.achievement_template where a.title == e.UniqueData select a.id).FirstOrDefault();
			}

			// Put in requirements
			foreach (Dictionary<String, String> row in reqData)
			{
				// Get this achievement
				ImportedEarnable achieve = GetImportedEarnableByOldID(_achievementMap, row["achievementID"]);
				if (achieve == null || achieve.NewID == 0)
					continue;

				work.EntityContext.achievement_requirement.Add(new achievement_requirement()
				{
					achievement_id = achieve.NewID,
					description = row["description"]
				});
			}
			work.SaveChanges();

			// Put in caretakers
			foreach (Dictionary<String, String> row in caretakerData)
			{
				// Get this achievement and user
				ImportedEarnable achieve = GetImportedEarnableByOldID(_achievementMap, row["achievementID"]);
				if (achieve == null || achieve.NewID == 0)
					continue;
				ImportedUser user = GetImportedUserByOldID(row["caretakerID"]);
				if (user == null || user.NewID == 0)
					continue;

				work.EntityContext.achievement_caretaker.Add(new achievement_caretaker()
				{
					achievement_id = achieve.NewID,
					caretaker_id = user.NewID
				});
			}
			work.SaveChanges();
		}

		/// <summary>
		/// Imports the user content from V2
		/// </summary>
		/// <param name="userStoriesTable">User Story data</param>
		/// <param name="userContentTable">User Content data</param>
		/// <param name="delimiter">Delimiter between data</param>
		/// <param name="work">The DB access</param>
		private void ImportAchievementUserStuff(
			HttpPostedFileBase userStoriesTable,
			HttpPostedFileBase userContentTable,
			String delimiter,
			UnitOfWork work)
		{
			// Grab the data
			List<Dictionary<String, String>> storyData = GetDataFromFile(userStoriesTable, delimiter);
			if (storyData == null)
			{
				ModelState.AddModelError("", "Error with User Story table.  Check Debug Output");
				return;
			}

			List<Dictionary<String, String>> contentData = GetDataFromFile(userContentTable, delimiter);
			if (contentData == null)
			{
				ModelState.AddModelError("", "Error with User Content table.  Check Debug Output");
				return;
			}

			// Go through stories
			foreach (Dictionary<String, String> row in storyData)
			{
				int oldID = int.Parse(row["userstoryID"]);
				achievement_user_story story = new achievement_user_story()
				{
					date_submitted = DateTime.Parse(row["date_submitted"]),
					image = row["uc_url"],
					text = row["uc_text"]
				};
				ImportedEarnable impStory = new ImportedEarnable()
				{
					OldID = oldID,
					UniqueData = row["date_submitted"]
				};

				work.EntityContext.achievement_user_story.Add(story);
				_userStoryMap.Add(impStory.OldID, impStory);
			}

			// Save, then get new ids
			work.SaveChanges();
			foreach (ImportedEarnable impStory in _userStoryMap.Values)
			{
				DateTime submitTime = DateTime.Parse(impStory.UniqueData);
				impStory.NewID = (from s in work.EntityContext.achievement_user_story where s.date_submitted == submitTime select s.id).FirstOrDefault();
			}

			// Go through content
			foreach (Dictionary<String, String> row in contentData)
			{
				// Get the user
				ImportedUser approver = GetImportedUserByOldID(row["approverID"]);
				if (approver == null)
					continue;

				// Make the data entry
				int oldID = int.Parse(row["usercontentID"]);
				achievement_user_content content = new achievement_user_content()
				{
					submitted_date = DateTime.Parse(row["date_submitted"]),
					text = row["uc_text"],
					approved_by_id = approver.NewID,
					approved_date = DateTime.Parse(row["date_handled"])
				};

				// content type
				// 1 - image
				// 2 - url
				// 3 - text
				int contentType = int.Parse(row["typeID"]);
				switch (contentType)
				{
					case 1: // image
						content.content_type = (int)JPPConstants.UserSubmissionTypes.Image;
						content.image = row["uc_url"];
						break;

					case 2: // url
						content.content_type = (int)JPPConstants.UserSubmissionTypes.URL;
						content.url = row["uc_url"];
						break;

					case 3: // text
						content.content_type = (int)JPPConstants.UserSubmissionTypes.Text;
						break;
				}

				// Make the map item
				ImportedEarnable impContent = new ImportedEarnable()
				{
					OldID = oldID,
					UniqueData = row["date_submitted"]
				};

				work.EntityContext.achievement_user_content.Add(content);
				_userContentMap.Add(impContent.OldID, impContent);
			}

			// Save, then get new ids
			work.SaveChanges();
			foreach (ImportedEarnable impStory in _userContentMap.Values)
			{
				DateTime submitTime = DateTime.Parse(impStory.UniqueData);
				impStory.NewID = (from s in work.EntityContext.achievement_user_content where s.submitted_date == submitTime select s.id).FirstOrDefault();
			}
		}


		/// <summary>
		/// Imports the achievement instances from V2
		/// </summary>
		/// <param name="achieveTemplateTable">File w/ achievement instance data</param>
		/// <param name="achievePointTemplateTable">Achievement point instance data (to be rolled into achievement)</param>
		/// <param name="delimiter">Delimiter between data</param>
		/// <param name="work">The DB access</param>
		private void ImportAchievementInstances(
			HttpPostedFileBase achieveInstanceTable,
			HttpPostedFileBase achievePointInstanceTable,
			String delimiter,
			UnitOfWork work)
		{
			// Grab the data
			List<Dictionary<String, String>> achieveData = GetDataFromFile(achieveInstanceTable, delimiter);
			if (achieveData == null)
			{
				ModelState.AddModelError("", "Error with Achievement Instance table.  Check Debug Output");
				return;
			}

			List<Dictionary<String, String>> pointData = GetDataFromFile(achievePointInstanceTable, delimiter);
			if (pointData == null)
			{
				ModelState.AddModelError("", "Error with Achievement Point Instance table.  Check Debug Output");
				return;
			}

			// Put points into a dictionary
			Dictionary<int, int[]> pointLookUp = new Dictionary<int, int[]>();
			foreach (Dictionary<String, String> row in pointData)
			{
				int oldID = int.Parse(row["achievement_instanceID"]);
				if (!pointLookUp.ContainsKey(oldID))
				{
					pointLookUp.Add(oldID, new int[5]);
				}
				pointLookUp[oldID][int.Parse(row["categoryID"])] = int.Parse(row["points"]);
			}

			// Go through each data row
			foreach (Dictionary<String, String> row in achieveData)
			{
				// Get the user and achievement
				ImportedEarnable achieve = GetImportedEarnableByOldID(_achievementMap, row["achievementID"]);
				ImportedUser user = GetImportedUserByOldID(row["userID"]);
				ImportedUser assigner = GetImportedUserByOldID(row["assignedbyID"]);
				if (achieve == null || achieve.NewID == 0 ||
					user == null || user.NewID == 0)
					continue;

				// How many?
				int count = int.Parse(row["achievementcount"]);
				if (count == 0)
					count = 1;

				// Make multiple if it's repeatable!
				for (int i = 0; i < count; i++)
				{
					achievement_instance instance = new achievement_instance()
					{
						achieved_date = DateTime.Parse(row["date_achieved"]),
						achievement_id = achieve.NewID,
						assigned_by_id = assigner == null ? user.NewID : assigner.NewID,
						card_given = Boolean.Parse(row["has_cardbeengiven"]),
						card_given_date = String.IsNullOrWhiteSpace(row["givenDate"]) ? (DateTime?)null : DateTime.Parse(row["givenDate"]),
						comments_disabled = false,
						has_user_content = Boolean.Parse(row["has_usercontent"]),
						has_user_story = Boolean.Parse(row["has_userstory"]),
						user_id = user.NewID
					};

					if (i == 0)
					{
						// Look up points
						int[] points;
						if (pointLookUp.TryGetValue(int.Parse(row["achievement_instanceID"]), out points))
						{
							instance.points_create = points[1]; // Create = 1
							instance.points_explore = points[4]; // Explore = 4
							instance.points_learn = points[2]; // Learn = 2
							instance.points_socialize = points[3]; // Socialize = 3
						}

						// Get user content/story stuff
						ImportedEarnable userContent = GetImportedEarnableByOldID(_userContentMap, row["usercontentID"]);
						ImportedEarnable userStory = GetImportedEarnableByOldID(_userStoryMap, row["userstoryID"]);
						if (instance.has_user_content)
						{
							if (userContent == null || userContent.NewID == 0)
								continue;	// If content is REQUIRED, and not found, skip this - User will need to re-get achievement
							else
								instance.user_content_id = userContent.NewID;
						}

						// Check for legit user story
						if (instance.has_user_story && userStory != null && userStory.NewID > 0)
						{
							instance.user_story_id = userStory.NewID;
						}
					}

					// Add to the DB
					work.EntityContext.achievement_instance.Add(instance);
				}

			}

			work.SaveChanges();
		}

		#endregion

		#region Quests

		/// <summary>
		/// Imports the quest templates from V2
		/// </summary>
		/// <param name="questTemplateTable">File w/ quest template data</param>
		/// <param name="questAchievementStepTable">Achievement steps data</param>
		/// <param name="delimiter">Delimiter between data</param>
		/// <param name="work">The DB access</param>
		private void ImportQuestTemplates(
			HttpPostedFileBase questTemplateTable,
			HttpPostedFileBase questAchievementStepTable,
			String delimiter,
			UnitOfWork work)
		{
			// Grab the data
			List<Dictionary<String, String>> questData = GetDataFromFile(questTemplateTable, delimiter);
			if (questData == null)
			{
				ModelState.AddModelError("", "Error with Quest Template table.  Check Debug Output");
				return;
			}

			List<Dictionary<String, String>> stepData = GetDataFromFile(questAchievementStepTable, delimiter);
			if (stepData == null)
			{
				ModelState.AddModelError("", "Error with Quest Step table.  Check Debug Output");
				return;
			}

			// Loop through quests
			foreach (Dictionary<String, String> row in questData)
			{
				// Get the creator
				ImportedUser creator = GetImportedUserByOldID(row["creatorID"]);
				if (creator == null || creator.NewID == 0)
					continue;

				ImportedUser modifiedBy = GetImportedUserByOldID(row["last_modified_by"]);
				if (modifiedBy == null || modifiedBy.NewID == 0)
					continue;

				int oldID = int.Parse(row["questID"]);
				int threshold = int.Parse(row["quest_threshhold"]);
				quest_template quest = new quest_template()
				{
					created_date = DateTime.Parse(row["date_created"]),
					creator_id = creator.NewID,
					description = row["description"],
					featured = Boolean.Parse(row["is_featured"]),
					icon = row["icon"],
					last_modified_by_id = modifiedBy.NewID,
					last_modified_date = DateTime.Parse(row["date_modified"]),
					posted_date = DateTime.Parse(row["date_posted"]),
					retire_date = null,
					state = (int)JPPConstants.AchievementQuestStates.Inactive,
					threshold = threshold <= 0 ? (int?)null : threshold,
					title = row["title"],
					user_generated = false
				};

				ImportedEarnable impQuest = new ImportedEarnable()
				{
					OldID = oldID,
					UniqueData = quest.title
				};

				work.EntityContext.quest_template.Add(quest);
				_questMap.Add(impQuest.OldID, impQuest);
			}

			work.SaveChanges();

			foreach (ImportedEarnable impQuest in _questMap.Values)
			{
				impQuest.NewID = (from q in work.EntityContext.quest_template where q.title == impQuest.UniqueData select q.id).FirstOrDefault();
			}

			// Achievement steps
			foreach (Dictionary<String, String> row in stepData)
			{
				// Get the achievement and quest
				ImportedEarnable quest = GetImportedEarnableByOldID(_questMap, row["questID"]);
				if (quest == null || quest.NewID == 0)
					continue;
				ImportedEarnable achieve = GetImportedEarnableByOldID(_achievementMap, row["achievementID"]);
				if (achieve == null || achieve.NewID == 0)
					continue;

				quest_achievement_step step = new quest_achievement_step()
				{
					achievement_id = achieve.NewID,
					quest_id = quest.NewID
				};

				work.EntityContext.quest_achievement_step.Add(step);
			}
			work.SaveChanges();
		}

		/// <summary>
		/// Imports the quest instances from V2
		/// </summary>
		/// <param name="questInstanceTable">File w/ quest template data</param>
		/// <param name="delimiter">Delimiter between data</param>
		/// <param name="work">The DB access</param>
		private void ImportQuestInstances(
			HttpPostedFileBase questInstanceTable,
			String delimiter,
			UnitOfWork work)
		{
			// Grab the data
			List<Dictionary<String, String>> questData = GetDataFromFile(questInstanceTable, delimiter);
			if (questData == null)
			{
				ModelState.AddModelError("", "Error with Quest Instance table.  Check Debug Output");
				return;
			}

			// Loop through quests
			foreach (Dictionary<String, String> row in questData)
			{
				ImportedUser user = GetImportedUserByOldID(row["userID"]);
				if (user == null || user.NewID == 0)
					continue;

				ImportedEarnable quest = GetImportedEarnableByOldID(_questMap, row["questID"]);
				if (quest == null || quest.NewID == 0)
					continue;

				quest_instance instance = new quest_instance()
				{
					comments_disabled = false,
					completed_date = DateTime.Parse(row["date_completed"]),
					quest_id = quest.NewID,
					user_id = user.NewID
				};

				work.EntityContext.quest_instance.Add(instance);
			}

			work.SaveChanges();
		}

		#endregion
	}
}
