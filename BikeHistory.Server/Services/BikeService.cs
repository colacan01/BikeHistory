using BikeHistory.Server.Data;
using BikeHistory.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace BikeHistory.Server.Services
{
    public class BikeService
    {
        private readonly ApplicationDbContext _context;
        // Update the field type for _userManager to the correct type
        private readonly UserManager<ApplicationUser> _userManager;

        // Update the constructor to accept UserManager<ApplicationUser> as a dependency
        public BikeService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Get all bike frames
        public async Task<List<BikeFrame>> GetAllBikeFramesAsync()
        {
            return await _context.BikeFrames
                .Include(b => b.Manufacturer)
                .Include(b => b.Brand)
                .Include(b => b.BikeType)
                .Include(b => b.CurrentOwner)
                .ToListAsync();
        }

        // Get bike frames by owner
        public async Task<List<BikeFrame>> GetBikeFramesByOwnerIdAsync(string ownerId)
        {
            if (string.IsNullOrEmpty(ownerId))
            {
                throw new ArgumentException("Owner ID cannot be null or empty", nameof(ownerId));
            }
            
            return await _context.BikeFrames
                .Include(b => b.Manufacturer)
                .Include(b => b.Brand)
                .Include(b => b.BikeType)
                .Where(b => b.CurrentOwnerId == ownerId)
                .ToListAsync();
        }

        // Get bike frame by id
        public async Task<BikeFrame?> GetBikeFrameByIdAsync(int id)
        {
            return await _context.BikeFrames
                .Include(b => b.Manufacturer)
                .Include(b => b.Brand)
                .Include(b => b.BikeType)
                .Include(b => b.CurrentOwner)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        // Get bike frame by frame number
        public async Task<BikeFrame?> GetBikeFrameByFrameNumberAsync(string frameNumber)
        {
            if (string.IsNullOrEmpty(frameNumber))
            {
                throw new ArgumentException("Frame number cannot be null or empty", nameof(frameNumber));
            }
            
            return await _context.BikeFrames
                .Include(b => b.Manufacturer)
                .Include(b => b.Brand)
                .Include(b => b.BikeType)
                .Include(b => b.CurrentOwner)
                .FirstOrDefaultAsync(b => b.FrameNumber == frameNumber);
        }

        // Register a new bike frame
        public async Task<BikeFrame> RegisterBikeFrameAsync(BikeFrame bikeFrame)
        {
            // Check if frame number already exists
            var existingFrame = await _context.BikeFrames
                .FirstOrDefaultAsync(b => b.FrameNumber == bikeFrame.FrameNumber);

            if (existingFrame != null)
            {
                throw new InvalidOperationException("Bike frame with this frame number already exists.");
            }

            _context.BikeFrames.Add(bikeFrame);
            await _context.SaveChangesAsync();
            return bikeFrame;
        }

        // Update bike frame
        public async Task<BikeFrame> UpdateBikeFrameAsync(BikeFrame bikeFrame)
        {
            _context.Entry(bikeFrame).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return bikeFrame;
        }

        // Transfer ownership
        public async Task<OwnershipRecord> TransferOwnershipAsync(int bikeFrameId, string newOwnerId, string? notes = null)
        {
            if (string.IsNullOrEmpty(newOwnerId))
            {
                throw new ArgumentException("New owner ID cannot be null or empty", nameof(newOwnerId));
            }

            var newOwner = await _userManager.FindByEmailAsync(newOwnerId);
            if (newOwner == null)
            {
                throw new InvalidOperationException($"사용자 ID '{newOwnerId}'를 찾을 수 없습니다.");
            }

            var bikeFrame = await _context.BikeFrames
                .Include(b => b.CurrentOwner)
                .FirstOrDefaultAsync(b => b.Id == bikeFrameId);
            
            if (bikeFrame == null)
            {
                throw new InvalidOperationException("Bike frame not found.");
            }

            var previousOwnerId = bikeFrame.CurrentOwnerId;

            // Create ownership record
            var ownershipRecord = new OwnershipRecord
            {
                BikeFrameId = bikeFrameId,
                PreviousOwnerId = previousOwnerId,
                NewOwnerId = newOwner.Id,
                TransferDate = DateTime.UtcNow,
                Notes = notes
            };

            // Update bike frame with new owner
            bikeFrame.CurrentOwnerId = newOwner.Id;

            _context.OwnershipRecords.Add(ownershipRecord);
            _context.Entry(bikeFrame).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return ownershipRecord;
        }

        // Get ownership history for a bike frame
        public async Task<List<OwnershipRecord>> GetOwnershipHistoryAsync(int bikeFrameId)
        {
            return await _context.OwnershipRecords
                .Include(o => o.PreviousOwner)
                .Include(o => o.NewOwner)
                .Where(o => o.BikeFrameId == bikeFrameId)
                .OrderByDescending(o => o.TransferDate)
                .ToListAsync();
        }
    }
}