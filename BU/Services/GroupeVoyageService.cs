namespace BU.Services;

public class GroupeVoyageService : IGroupeVoyageService
{
    private readonly TravelPlannDbContext _context;

    public GroupeVoyageService(TravelPlannDbContext context)
    {
        _context = context;
    }

    public async Task<GroupeVoyage> CreateAsync(string nomGroupe)
    {
        var groupe = new GroupeVoyage
        {
            NomGroupe = nomGroupe,
            DateCreation = DateOnly.FromDateTime(DateTime.Now)
        };

        _context.GroupeVoyages.Add(groupe);
        await _context.SaveChangesAsync();
        return groupe;
    }

    public async Task<List<GroupeVoyage>> GetAllAsync()
    {
        return await _context.GroupeVoyages
            .Include(g => g.MembreGroupes)
            .ThenInclude(m => m.Utilisateur)
            .OrderBy(g => g.NomGroupe)
            .ToListAsync();
    }

    public async Task<GroupeVoyage?> GetByIdAsync(int id)
    {
        return await _context.GroupeVoyages
            .Include(g => g.MembreGroupes)
            .ThenInclude(m => m.Utilisateur)
            .FirstOrDefaultAsync(g => g.GroupeId == id);
    }

    public async Task UpdateAsync(GroupeVoyage groupe)
    {
        _context.GroupeVoyages.Update(groupe);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var groupe = await _context.GroupeVoyages.FindAsync(id);
        if (groupe != null)
        {
            _context.GroupeVoyages.Remove(groupe);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<MembreGroupe> AddMembreAsync(int groupeId, int utilisateurId, string role)
    {
        var membre = new MembreGroupe
        {
            GroupeId = groupeId,
            UtilisateurId = utilisateurId,
            Role = role,
            DateAdhesion = DateOnly.FromDateTime(DateTime.Now)
        };

        _context.MembreGroupes.Add(membre);
        await _context.SaveChangesAsync();
        return membre;
    }

    public async Task RemoveMembreAsync(int groupeId, int utilisateurId)
    {
        var membre = await _context.MembreGroupes
            .FirstOrDefaultAsync(m => m.GroupeId == groupeId && m.UtilisateurId == utilisateurId);

        if (membre != null)
        {
            _context.MembreGroupes.Remove(membre);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<MembreGroupe>> GetMembresAsync(int groupeId)
    {
        return await _context.MembreGroupes
            .Include(m => m.Utilisateur)
            .Where(m => m.GroupeId == groupeId)
            .ToListAsync();
    }

    public async Task<List<GroupeVoyage>> GetGroupesByUtilisateurAsync(int utilisateurId)
    {
        return await _context.GroupeVoyages
            .Where(g => g.MembreGroupes.Any(m => m.UtilisateurId == utilisateurId))
            .Include(g => g.MembreGroupes)
            .ThenInclude(m => m.Utilisateur)
            .ToListAsync();
    }
}