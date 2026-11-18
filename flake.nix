{
  description = "BetterStrap, a Roblox bootstrapper that launches Roblox through Sober on Linux";

  inputs.nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
  inputs.self.submodules = true;

  outputs =
    { self, nixpkgs }:
    let
      systems = [
        "x86_64-linux"
        "aarch64-linux"
      ];
      forAllSystems = f: nixpkgs.lib.genAttrs systems (system: f nixpkgs.legacyPackages.${system});
      version = builtins.head (
        builtins.elemAt (builtins.split "<BetterStrapVersion>([0-9.]+)</BetterStrapVersion>" (builtins.readFile ./Directory.Build.props)) 1
      );
    in
    {
      packages = forAllSystems (pkgs: rec {
        betterstrap = pkgs.callPackage ./nix/package.nix { };
        betterstrap-git = betterstrap.overrideAttrs {
          inherit version;
          src = self;
        };
        default = betterstrap;
      });

      nixosModules.default = import ./nix/nixos-module.nix self;
      nixosModules.betterstrap = self.nixosModules.default;

      homeModules.default = import ./nix/home-module.nix self;
      homeModules.betterstrap = self.homeModules.default;
      homeManagerModules = self.homeModules;

      overlays.default = final: prev: {
        betterstrap = self.packages.${final.stdenv.hostPlatform.system}.betterstrap;
      };

      devShells = forAllSystems (pkgs: {
        default = pkgs.mkShell {
          inputsFrom = [ self.packages.${pkgs.stdenv.hostPlatform.system}.betterstrap-git ];
          packages = [
            pkgs.cargo
            pkgs.rustc
          ];
        };
      });

      formatter = forAllSystems (pkgs: pkgs.nixfmt);
    };
}
