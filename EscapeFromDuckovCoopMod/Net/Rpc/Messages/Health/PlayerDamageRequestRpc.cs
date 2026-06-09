using LiteNetLib;
using LiteNetLib.Utils;

namespace EscapeFromDuckovCoopMod;

[Rpc(Op.PLAYER_DAMAGE_REQUEST, DeliveryMethod.ReliableOrdered, RpcDirection.ClientToServer)]
public struct PlayerDamageRequestRpc : IRpcMessage
{
    public string TargetPlayerId;
    public DamageForwardPayload Damage;
    public string AttackerPlayerId;

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(TargetPlayerId ?? string.Empty);
        Damage.Serialize(writer);
        writer.Put(AttackerPlayerId ?? string.Empty);
    }

    public void Deserialize(NetPacketReader reader)
    {
        TargetPlayerId = reader.GetString();
        Damage = default;
        Damage.Deserialize(reader);
        AttackerPlayerId = reader.AvailableBytes > 0 ? reader.GetString() : string.Empty;
    }
}
