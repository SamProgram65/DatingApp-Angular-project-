using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void AddMessage(Messages message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Messages message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Messages> GetMessage(int id)
        {
            return await _context.Messages
            .Include(u => u.Sender)
            .Include(u => u.Recipient)
            .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages
             .OrderByDescending(m => m.MessageSent)
             .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.Recipient.UserName == messageParams.Username 
                && u.RecipientDeleted == false),
                "Outbox" => query.Where(u => u.Sender.UserName == messageParams.Username
                && u.SenderDeleted == false),
                _ => query.Where(u => u.Recipient.UserName ==
                    messageParams.Username && u.RecipientDeleted == false && u.DateRead == null)
            };

            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername,
         string recipientUsername)
        {
            var message = await _context.Messages
             .Include(u => u.Sender).ThenInclude(p => p.Photos)
             .Include(u => u.Recipient).ThenInclude(p => p.Photos)
             .Where(m => m.Recipient.UserName == currentUsername  && m.RecipientDeleted == false
                       && m.Sender.UserName == recipientUsername
                       || m.Recipient.UserName == recipientUsername
                       && m.Sender.UserName == currentUsername && m.SenderDeleted == false
             )
              .OrderBy(m => m.MessageSent)
              .ToListAsync();

              var unreadMessages = message.Where(m => m.DateRead == null
              && m.Recipient.UserName == currentUsername).ToList();

              if(unreadMessages.Any())
              {
                  foreach ( var messages in unreadMessages)
                  {
                      messages.DateRead = DateTime.Now;
                  }

                  await _context.SaveChangesAsync();
              }  

              return _mapper.Map<IEnumerable<MessageDto>>(message);
        }

        

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}