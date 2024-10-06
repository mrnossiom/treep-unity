{
  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-24.05";
  };

  outputs = { self, nixpkgs }:
    let
      inherit (nixpkgs.lib) genAttrs getName;

      forAllSystems = genAttrs [ "x86_64-linux" "aarch64-linux" "aarch64-darwin" ];
      forAllPkgs = function: forAllSystems (system: function pkgs.${system});

      pkgs = forAllSystems (system: (import nixpkgs {
        inherit system;
        config.allowUnfreePredicate =
          pkg: builtins.elem (getName pkg) [ "rider" ];
        overlays = [ ];
      }));

      cs-pkgs = pkgs: [
        pkgs.dotnetCorePackages.sdk_9_0
        pkgs.msbuild
      ];
    in
    {
      formatter = forAllPkgs (pkgs: pkgs.nixpkgs-fmt);

      packages = forAllPkgs (pkgs: {
        rider = pkgs.jetbrains.rider.overrideAttrs (old: {
          nativeBuildInputs = old.nativeBuildInputs ++ cs-pkgs pkgs;
        });
      });

      devShells = forAllPkgs (pkgs:
        with pkgs.lib;
        let
          # key = val
        in
        {
          default = pkgs.mkShell rec {
            nativeBuildInputs = with pkgs; [
              # lsp
              omnisharp-roslyn
              netcoredbg
            ] ++ cs-pkgs pkgs;

            buildInputs = with pkgs; [
              # openssl
            ];

            LD_LIBRARY_PATH = makeLibraryPath buildInputs;
          };
        });
    };
}
