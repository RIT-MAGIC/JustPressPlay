using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using JustPressPlay.Utilities;
using JustPressPlay.ViewModels;

namespace JustPressPlay.Controllers
{
	/// <summary>
	/// Handles all JSON requests.
	/// </summary>
	[Authorize]
    public class JSONController : Controller
    {
		/// <summary>
		/// Returns a list of players
		/// TODO: Add parameters for filtering (and sorting?)
		/// </summary>
		/// <returns>GET: /JSON/Players</returns>
		public String Players()
		{
			// Get the player list
			return PlayersListViewModel.Populate().ToJSON();
		}
    }
}
