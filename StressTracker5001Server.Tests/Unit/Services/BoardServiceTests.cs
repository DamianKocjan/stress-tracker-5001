using Xunit;
using Microsoft.EntityFrameworkCore;
using StressTracker5001Server.Services;
using StressTracker5001Server.Data;
using StressTracker5001Server.Models;
using StressTracker5001Server.DTOs.Board;
using StressTracker5001Server.Tests.Helpers;

namespace StressTracker5001Server.Tests.Unit.Services;

public class BoardServiceTests : IDisposable
{
	private readonly AppDbContext _context;
	private readonly BoardAuthorizationService _authService;
	private readonly BoardService _boardService;

	public BoardServiceTests()
	{
		_context = TestDbContextFactory.CreateInMemoryDbContext();
		_authService = new BoardAuthorizationService(_context);
		_boardService = new BoardService(_context, _authService);
	}

	public void Dispose()
	{
		_context.Database.EnsureDeleted();
		_context.Dispose();
	}

	[Fact]
	public async Task GetBoardByIdAsync_AsOwner_ReturnsBoard()
	{
		// Arrange
		var user = TestDataFactory.CreateTestUser();
		_context.Users.Add(user);
		await _context.SaveChangesAsync();

		var board = TestDataFactory.CreateTestBoard(user.Id);
		_context.Boards.Add(board);
		await _context.SaveChangesAsync();

		// Create owner as a board member (simulates what CreateBoardAsync does)
		var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Owner);
		_context.BoardMembers.Add(ownerMember);
		await _context.SaveChangesAsync();

		// Act
		var result = await _boardService.GetBoardByIdAsync(board.Id, user.Id);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);
		Assert.Equal(board.Id, result.Value.Id);
	}

	[Fact]
	public async Task GetBoardByIdAsync_AsMember_ReturnsBoard()
	{
		// Arrange
		var owner = TestDataFactory.CreateTestUser(email: "owner@example.com");
		var member = TestDataFactory.CreateTestUser(email: "member@example.com");
		_context.Users.AddRange(owner, member);
		await _context.SaveChangesAsync();

		var board = TestDataFactory.CreateTestBoard(owner.Id);
		_context.Boards.Add(board);
		await _context.SaveChangesAsync();

		var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, member.Id, BoardMemberRole.Member);
		_context.BoardMembers.Add(boardMember);
		await _context.SaveChangesAsync();

		// Act
		var result = await _boardService.GetBoardByIdAsync(board.Id, member.Id);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);
		Assert.Equal(board.Id, result.Value.Id);
	}

	[Fact]
	public async Task GetBoardByIdAsync_AsNonMember_ReturnsForbidden()
	{
		// Arrange
		var owner = TestDataFactory.CreateTestUser(email: "owner@example.com");
		var nonMember = TestDataFactory.CreateTestUser(email: "nonmember@example.com");
		_context.Users.AddRange(owner, nonMember);
		await _context.SaveChangesAsync();

		var board = TestDataFactory.CreateTestBoard(owner.Id);
		_context.Boards.Add(board);
		await _context.SaveChangesAsync();

		// Act
		var result = await _boardService.GetBoardByIdAsync(board.Id, nonMember.Id);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Equal(403, result.StatusCode);
	}

	[Fact]
	public async Task CreateBoardAsync_WithValidData_CreatesBoard()
	{
		// Arrange
		var user = TestDataFactory.CreateTestUser();
		_context.Users.Add(user);
		await _context.SaveChangesAsync();

		var createDto = new CreateBoardDto
		{
			Name = "New Board",
			Description = "New Board Description"
		};

		// Act
		var result = await _boardService.CreateBoardAsync(createDto, user.Id);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.NotEqual(0, result.Value);

		// Verify board was created
		var board = await _context.Boards
			.Include(b => b.Members)
			.FirstOrDefaultAsync(b => b.Id == result.Value);
		Assert.NotNull(board);
		Assert.Equal(createDto.Name, board.Name);

		// Verify owner was created as a BoardMember with Owner role
		var ownerMember = board.Members.FirstOrDefault(m => m.Role == BoardMemberRole.Owner);
		Assert.NotNull(ownerMember);
		Assert.Equal(user.Id, ownerMember.UserId);
	}

	[Fact]
	public async Task UpdateBoardAsync_AsOwner_UpdatesBoard()
	{
		// Arrange
		var user = TestDataFactory.CreateTestUser();
		_context.Users.Add(user);
		await _context.SaveChangesAsync();

		var board = TestDataFactory.CreateTestBoard(user.Id, "Old Name");
		_context.Boards.Add(board);
		await _context.SaveChangesAsync();

		var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Admin);
		_context.BoardMembers.Add(ownerMember);
		await _context.SaveChangesAsync();

		var updateDto = new UpdateBoardDto
		{
			Name = "Updated Name",
			Description = "Updated Description"
		};

		// Act
		var result = await _boardService.UpdateBoardAsync(board.Id, updateDto, user.Id);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);
		Assert.Equal(updateDto.Name, result.Value.Name);
		Assert.Equal(updateDto.Description, result.Value.Description);
	}

	[Fact]
	public async Task UpdateBoardAsync_AsNonAdmin_ReturnsForbidden()
	{
		// Arrange
		var owner = TestDataFactory.CreateTestUser(email: "owner@example.com");
		var member = TestDataFactory.CreateTestUser(email: "member@example.com");
		_context.Users.AddRange(owner, member);
		await _context.SaveChangesAsync();

		var board = TestDataFactory.CreateTestBoard(owner.Id);
		_context.Boards.Add(board);
		await _context.SaveChangesAsync();

		// Member is not Admin, so should be forbidden
		var boardMember = TestDataFactory.CreateTestBoardMember(board.Id, member.Id, BoardMemberRole.Member);
		_context.BoardMembers.Add(boardMember);
		await _context.SaveChangesAsync();

		var updateDto = new UpdateBoardDto
		{
			Name = "Updated Name"
		};

		// Act
		var result = await _boardService.UpdateBoardAsync(board.Id, updateDto, member.Id);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Equal(403, result.StatusCode);
	}

	[Fact]
	public async Task DeleteBoardAsync_AsOwner_DeletesBoard()
	{
		// Arrange
		var user = TestDataFactory.CreateTestUser();
		_context.Users.Add(user);
		await _context.SaveChangesAsync();

		var board = TestDataFactory.CreateTestBoard(user.Id);
		_context.Boards.Add(board);
		await _context.SaveChangesAsync();

		// Owner must have Owner role, not Admin
		var ownerMember = TestDataFactory.CreateTestBoardMember(board.Id, user.Id, BoardMemberRole.Owner);
		_context.BoardMembers.Add(ownerMember);
		await _context.SaveChangesAsync();

		// Act
		var result = await _boardService.DeleteBoardAsync(board.Id, user.Id);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.True(result.Value);

		// Verify board is deleted
		var deletedBoard = await _context.Boards.FindAsync(board.Id);
		Assert.Null(deletedBoard);
	}

	[Fact]
	public async Task GetOwnedBoardsAsync_ReturnsOnlyOwnedBoards()
	{
		// Arrange
		var user = TestDataFactory.CreateTestUser();
		var otherUser = TestDataFactory.CreateTestUser(email: "other@example.com");
		_context.Users.AddRange(user, otherUser);
		await _context.SaveChangesAsync();

		var board1 = TestDataFactory.CreateTestBoard(user.Id, "Board 1");
		var board2 = TestDataFactory.CreateTestBoard(user.Id, "Board 2");
		var board3 = TestDataFactory.CreateTestBoard(otherUser.Id, "Board 3");
		_context.Boards.AddRange(board1, board2, board3);
		await _context.SaveChangesAsync();

		// Create owners as board members
		var owners = new List<BoardMember>
		{
			TestDataFactory.CreateTestBoardMember(board1.Id, user.Id, BoardMemberRole.Owner),
			TestDataFactory.CreateTestBoardMember(board2.Id, user.Id, BoardMemberRole.Owner),
			TestDataFactory.CreateTestBoardMember(board3.Id, otherUser.Id, BoardMemberRole.Owner)
		};
		_context.BoardMembers.AddRange(owners);
		await _context.SaveChangesAsync();

		// Act
		var result = await _boardService.GetOwnedBoardsAsync(user.Id);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);
		Assert.Equal(2, result.Value.Count);

		// Verify all boards have user as Owner role member
		foreach (var board in result.Value)
		{
			var ownerMember = board.Members.FirstOrDefault(m => m.Role == BoardMemberRole.Owner);
			Assert.NotNull(ownerMember);
			Assert.Equal(user.Id, ownerMember.UserId);
		}
	}
}
