using Discord;
using Discord.Addons.EmojiTools;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UtilityBot.Services.Configuration;
using UtilityBot.Services.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using System.Threading;
using UtilityBot.DTO;

namespace UtilityBot
{
    public class CommandHandler
    {

        private readonly IServiceProvider _provider;
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;
        private readonly Config _config;
        private readonly ILogger _logger;

        public System.Threading.Timer _timer;
        public CommandHandler(IServiceProvider provider)
        {
            _provider = provider;
            _client = _provider.GetService<DiscordSocketClient>();

            _commands = _provider.GetService<CommandService>();
            var log = _provider.GetService<LogAdaptor>();
            _commands.Log += log.LogCommand;
            _config = _provider.GetService<Config>();
            _logger = _provider.GetService<Logger>().ForContext<CommandService>();

            _timer = new System.Threading.Timer(Callback, true, 1000, System.Threading.Timeout.Infinite);

        }
        private void Callback(Object state)
        {
            CheckGame();
            // Long running operation
            _timer.Change(2000, Timeout.Infinite);
        }
        public void CheckGame()
        {
            Color myColor = new Color(25, 202, 226);
            string hex = myColor.R.ToString("X2") + myColor.G.ToString("X2") + myColor.B.ToString("X2");

            foreach (var guild in _client.Guilds)
            {
                var roles = guild.Roles;
                var users = guild.Users;
                List<Roles> Rooles = new List<DTO.Roles>();

                foreach (var role in guild.Roles)
                {
                    Roles roleitem = new Roles();
                    roleitem.Role = role;
                    List<IGuildUser> Roleusers = new List<IGuildUser>();

                    foreach (var user in users)
                    {
                        bool contains = false;
                        foreach (var userrole in user.Roles)
                        {
                            if (userrole.Id == role.Id)
                            {
                                contains = true;
                            }
                        }
                        if (contains)
                        {
                            Roleusers.Add(user);
                        }

                    }
                    roleitem.Users = Roleusers;
                    Rooles.Add(roleitem);
                }
                foreach (var role in Rooles)
                {
                    if (role.Users.Count == 0)
                    {
                        role.Role.DeleteAsync();
                    }
                }

                foreach (var user in users)
                {

                    bool FoundRoleServer = false;

                    bool FoundRole = false;
                    foreach (var role in user.Roles)
                    {
                        if (role.Name == user.Game.ToString())
                        {
                            FoundRole = true;
                            FoundRoleServer = true;

                        }

                        else if (role.Color.RawValue == myColor.RawValue)
                        {
                            user.RemoveRoleAsync(role);

                        }
                    }

                    foreach (var role in roles)
                    {
                        if (user.Roles.Contains(role))
                        {

                        }


                        if (!FoundRole)
                        {


                            if (role.Name == user.Game.ToString())
                            {
                                FoundRoleServer = true;
                                user.AddRoleAsync(role);
                            }
                            else
                            {

                            }

                        }
                    }
                    if (user.Game.ToString() != "" && !user.IsBot)
                    {
                        if (!FoundRoleServer)
                        {
                          

                            var roleperm = new GuildPermissions(
                                createInstantInvite: false,
                                sendMessages: false,
                                readMessageHistory: false,
                                readMessages: false,
                                changeNickname: false,
                                mentionEveryone: false,
                                useExternalEmojis : false,
                                addReactions:false,
                                connect:false,
                                useVoiceActivation:false,
                                sendTTSMessages: false, 
                                attachFiles: false,
                                embedLinks: false, 
                                speak: false, 
                                muteMembers: false, 
                                manageRoles: false);


                            var role = guild.CreateRoleAsync(user.Game.ToString(), roleperm, myColor);

                            var rolepropp = new RoleProperties();
                            rolepropp.Mentionable = true;
                            
                             role.Result.ModifyAsync(x =>
                            {
                                x.Mentionable = true;
                            });
                            
                            user.AddRoleAsync(role.Result);
                        }
                    }


                }
            }
        }

        public async Task ConfigureAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }
    }
}
