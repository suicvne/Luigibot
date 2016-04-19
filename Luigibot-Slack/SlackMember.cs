using LuigibotCommon.Integrations;
using System;

namespace LuigibotSlack
{
	public enum SlackRole
	{
		Bot, User, Admin, Owner
	}

	public class SlackMember : IMember
	{
		public string Name {get;set;}
		public string ID {get;set;}
		public SlackRole Role { get; set;}

		public string RealName { get; set;}
		public string FirstName { get; set; }
		public string LastName { get; set; }

        public string Mention()
        {
            return $"<@{ID}>";
        }

		public SlackMember (SlackAPI.User user)
		{
			Name = user.name;
			ID = user.id;

			RealName = user.profile.real_name;
			FirstName = user.profile.first_name;
			LastName = user.profile.last_name;

			if (user.is_admin) {
				if (user.is_owner)
					Role = SlackRole.Owner;
				else
					Role = SlackRole.Admin;
			} else if (user.IsSlackBot)
				Role = SlackRole.Bot;
			else
				Role = SlackRole.User;
		}
	}
}

