using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

using JustPressPlay.Models.Repositories;

namespace JustPressPlay.ViewModels
{
	[DataContract]
	public class AchievementViewModel
	{
		[DataMember]
		public int ID { get; set; }

		[DataMember]
		public String Title { get; set; }

		[DataMember]
		public String Image { get; set; }
		
		[DataMember]
		public String Description { get; set; }

		[DataMember]
		public List<String> Requirements { get; set; }

		[DataMember]
		public List<AssociatedQuest> AssociatedQuests { get; set; }

		[DataMember]
		public int PointsCreate { get; set; }

		[DataMember]
		public int PointsExplore { get; set; }

		[DataMember]
		public int PointsLearn { get; set; }

		[DataMember]
		public int PointsSocialize { get; set; }

		[DataContract]
		public class AssociatedQuest
		{
			[DataMember]
			public int ID;

			[DataMember]
			public String Title;
		}

		public static AchievementViewModel Populate(int id, UnitOfWork work = null)
		{
			if (work == null) work = new UnitOfWork();

			// TODO: Finish

			return new AchievementViewModel()
			{

			};
		}
	}
}