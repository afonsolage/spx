using System.Collections.Generic;

public enum ChunkAction
{
    NONE,
    CREATE,
    ATTACH,
    DETACH,
    
    CHANGE_STAGE,
    NFY_STAGE_CHANGED,

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

public class ChunkChangeStageMessage : ChunkMessage
{
    public readonly ChunkStage stage;

    public ChunkChangeStageMessage(Vec3 pos, ChunkStage stage) : base(pos, ChunkAction.CHANGE_STAGE)
    {
        this.stage = stage;
    }
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
    public readonly List<Vec3> voxels;

    public ChunkReqVoxMessage(Vec3 pos, Vec3 target, List<Vec3> voxels) : base(pos, ChunkAction.REQ_VOX, target)
    {
        this.voxels = voxels;
    }

    public override ChunkMessage ToChunkNotFoundMessage()
    {
        return new ChunkResVoxMessage(this, null);
    }
}

public class ChunkResVoxMessage : ChunkToChunkMessage
{
    public readonly List<KeyValuePair<Vec3, VoxSnap>> list;

    public ChunkResVoxMessage(ChunkReqVoxMessage req, List<KeyValuePair<Vec3, VoxSnap>> list) : base(req.target, ChunkAction.RES_VOX, req.pos)
    {
        this.list = list;
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
    public ChunkResSunlightMessage(ChunkReqSunlightMessage req, byte[,] data) : base(req.target, ChunkAction.RES_SUNLIGHT, req.pos)
    {
        this.data = data;
    }
}

public class ChunkNotifyStageChanged : ChunkToChunkMessage
{
    public readonly ChunkStage stage;

    public ChunkNotifyStageChanged(Vec3 pos, Vec3 target, ChunkStage stage) : base(pos, ChunkAction.NFY_STAGE_CHANGED, target)
    {
        this.stage = stage;
    }
}