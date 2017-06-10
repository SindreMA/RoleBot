using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace UtilityBot.DTO
{
    public class Roles

    {
        public IRole Role { get; set; }
        public List<IGuildUser> Users { get; set; }
    }
}
