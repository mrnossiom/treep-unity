using Unity.Netcode.Components;

namespace KrommProject.Network {
    public class ClientNetworkTransform : NetworkTransform {
        protected override bool OnIsServerAuthoritative() {
            return false;
        }
    }
}
