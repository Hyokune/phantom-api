using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhantomAPI.Models;

namespace PhantomAPI.Helpers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhantomController : ControllerBase
    {
        private readonly PhantomAPIContext _context;

        public PhantomController(PhantomAPIContext context)
        {
            _context = context;
        }

        // GET: api/Phantom
        [HttpGet]
        public IEnumerable<PhantomThread> GetPhantomThread()
        {
            return _context.PhantomThread;
        }

        // GET: api/Phantom/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPhantomThread([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var phantomThread = await _context.PhantomThread.FindAsync(id);

            if (phantomThread == null)
            {
                return NotFound();
            }

            return Ok(phantomThread);
        }

        // PUT: api/Phantom/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPhantomThread([FromRoute] int id, [FromBody] PhantomThread phantomThread)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != phantomThread.Id)
            {
                return BadRequest();
            }

            _context.Entry(phantomThread).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PhantomThreadExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Phantom
        [HttpPost]
        public async Task<IActionResult> PostPhantomThread([FromBody] PhantomThread phantomThread)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.PhantomThread.Add(phantomThread);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPhantomThread", new { id = phantomThread.Id }, phantomThread);
        }

        // DELETE: api/Phantom/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhantomThread([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var phantomThread = await _context.PhantomThread.FindAsync(id);
            if (phantomThread == null)
            {
                return NotFound();
            }

            _context.PhantomThread.Remove(phantomThread);
            await _context.SaveChangesAsync();

            return Ok(phantomThread);
        }

        private bool PhantomThreadExists(int id)
        {
            return _context.PhantomThread.Any(e => e.Id == id);
        }
    }
}