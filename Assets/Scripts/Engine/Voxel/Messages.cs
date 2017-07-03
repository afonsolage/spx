public enum ChunkAction
{
    NONE,
    CREATE,
    LOAD,
    SETUP,
    LIGHT_PREPARE,
    LIGHT_SMOOTH,
    BUILD,
    ATTACH,
    DETACH,

    REQ_VOX,
    RES_VOX,

    REQ_SUNLIGHT,
    RES_SUNLIGHT,
}

public class ChunkMessage
{
    public readonly Vec3 pos;
    public readonly ChunkAction action;

    public ChunkMessage(Vec3 pos, ChunkAction action) { this.pos = pos; this.action = action; }
}

public class ChunkAttachMessage : ChunkMessage
{
    public readonly PrebuiltMesh mesh;

    public ChunkAttachMessage(Vec3 pos, PrebuiltMesh mesh) : base(pos, ChunkAction.ATTACH)
    {
        this.mesh = mesh;
    }
}

public class ChunkNoneMessage : ChunkMessage
{
    public ChunkNoneMessage(Vec3 pos) : base(pos, ChunkAction.NONE) { }
}

public class ChunkToChunkMessage : ChunkMessage
{
    public readonly Vec3 target;
    public ChunkToChunkMessage(Vec3 pos, ChunkAction action, Vec3 target) : base(pos, action)
    {
        this.target = target;
    }

    public virtual ChunkMessage ToChunkNotFoundMessage()
    {
        return new ChunkNoneMessage(pos);
    }
}

public class ChunkReqVoxMessage : ChunkToChunkMessage
{
    public readonly Vec3 vox;

    public ChunkReqVoxMessage(Vec3 pos, Vec3 target, Vec3 vox) : base(pos, ChunkAction.REQ_VOX, target)
    {
        this.vox = vox;
    }

    public override ChunkMessage ToChunkNotFoundMessage()
    {
        return new ChunkResVoxMessage(this, null);
    }
}

public class ChunkResVoxMessage : ChunkToChunkMessage
{
    public readonly Vec3 vox;
    public readonly VoxSnap snap;

    public ChunkResVoxMessage(ChunkReqVoxMessage req, VoxSnap snap) : base(req.target, ChunkAction.RES_VOX, req.pos)
    {
        this.vox = req.vox;
        this.snap = snap;
    }
}

public class ChunkReqSunlightMessage : ChunkToChunkMessage
{
    public ChunkReqSunlightMessage(Vec3 pos, Vec3 target) : base(pos, ChunkAction.REQ_SUNLIGHT, target) { }

    public override ChunkMessage ToChunkNotFoundMessage()
    {
        return new ChunkResSunlightMessage(this, null);
    }
}

public class ChunkResSunlightMessage : ChunkToChunkMessage
{
    public readonly byte[,] data;
    public ChunkResSunlightMessage(ChunkReqSunlightMessage req, byte[,] data) : base(req.target, ChunkAction.REQ_SUNLIGHT, req.pos)
    {
        this.data = data;
    }
}