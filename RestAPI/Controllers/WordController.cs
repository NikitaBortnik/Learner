﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestAPI.Data;
using RestAPI.Interfaces;
using RestAPI.Models;
using RestAPI.Services;
using RestAPI.ViewModels;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RestAPI.Controllers
{
    [Route("api/[controller]")]
    public class WordController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public WordController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/values
        [HttpGet]
        public IActionResult Get() =>
            Ok(_context.Words
                .Where(w => w.AppUserId == _userManager.GetUserId(User)));

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            if (id == null)
            {
                return BadRequest("Id is null;");
            }

            return Ok(await Task.Run(() =>
                _context.Words.Where(x =>
                x.AppUserId == _userManager.GetUserId(User) &&
                x.Id == id)
            ));
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] WordViewModel value)
        {
            if (ModelState.IsValid && value != null)
            {
                var user = await _context.Users
                    .Where(u => u.Id == _userManager.GetUserId(User))
                    .Include(w => w.Words)
                    .Include(c => c.Collections)
                    .FirstAsync();

                user.Words.Add(new Word(value, _userManager.GetUserId(User)));

                var result = await _userManager.UpdateAsync(user);
                //_context.Users.Update(user);
                if (result.Succeeded)
                    await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] Word value)
        {
            if (ModelState.IsValid && id == value.Id)
            {
                if (await _context.Words.AnyAsync(w => w.Id == id))
                {
                    _context.Words.Update(value);
                    await _context.SaveChangesAsync();
                    return NoContent();
                }
                return BadRequest($"Entity with Id {id} not found;");
            }
            return BadRequest("Id mismatch or model error;");            
        }


        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var word = await _context.Words
                .FirstOrDefaultAsync(w => w.Id == id &&
                w.AppUserId == _userManager.GetUserId(User)
            );

            if (word != null)
            {
                _context.Remove(word);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound($"Could not find the word with Id {id};");
        }
    }
}