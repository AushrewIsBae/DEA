﻿using DEA.Database.Repository;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DEA.Services
{
    public static class ModuleMethods
    {
        public static bool IsMod(SocketCommandContext context, IGuildUser user)
        {
            if (user.GuildPermissions.Administrator) return true;
            var guild = GuildRepository.FetchGuild(context.Guild.Id);
            if (guild.ModRoles != null)
                foreach (var role in guild.ModRoles)
                    if (user.Guild.GetRole(Convert.ToUInt64(role.Value)) != null)
                        if (user.RoleIds.Any(x => x.ToString() == role.Value)) return true;
            return false;
        }

        public static async Task InformSubjectAsync(IUser moderator, string action, IUser subject, string reason)
        {
            try
            {
                var channel = await subject.CreateDMChannelAsync();
                if (reason == "No reason.")
                    await channel.SendMessageAsync($"{moderator} has attempted to {action.ToLower()} you.");
                else
                    await channel.SendMessageAsync($"{moderator} has attempted to {action.ToLower()} you for the following reason: \"{reason}\"");
            }
            catch { }
        }

        public static async Task Gamble(SocketCommandContext context, double bet, double odds, double payoutMultiplier)
        {
            var user = UserRepository.FetchUser(context);
            var guild = GuildRepository.FetchGuild(context.Guild.Id);
            if (context.Guild.GetTextChannel(guild.GambleId) != null && context.Channel.Id != guild.GambleId)
                throw new Exception($"You may only gamble in {context.Guild.GetTextChannel(guild.GambleId).Mention}!");
            if (bet < Config.BET_MIN) throw new Exception($"Lowest bet is {Config.BET_MIN}$.");
            if (bet > user.Cash) throw new Exception($"You do not have enough money. Balance: {user.Cash.ToString("C", Config.CI)}.");
            double roll = new Random().Next(1, 10001) / 100.0;
            if (roll >= odds)
            {
                await UserRepository.EditCashAsync(context, (bet * payoutMultiplier));
                await context.Channel.SendMessageAsync($"{context.User.Mention}, you rolled: {roll.ToString("N2")}. Congrats, you won " + 
                                                       $"{(bet * payoutMultiplier).ToString("C", Config.CI)}! Balance: {(user.Cash + (bet * payoutMultiplier)).ToString("C", Config.CI)}.");
            }
            else
            {
                await UserRepository.EditCashAsync(context, -bet);
                await context.Channel.SendMessageAsync($"{context.User.Mention}, you rolled: {roll.ToString("N2")}. Unfortunately, you lost " + 
                                                       $"{bet.ToString("C", Config.CI)}. Balance: {(user.Cash - bet).ToString("C", Config.CI)}.");
            }
        }
    }
}
