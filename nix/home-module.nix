self:
{
  config,
  lib,
  pkgs,
  ...
}:

let
  cfg = config.programs.betterstrap;
  desktopEntry = "io.github.KloBraticc.BetterStrap.desktop";
  schemes = [
    "x-scheme-handler/roblox"
    "x-scheme-handler/roblox-player"
    "x-scheme-handler/roblox-studio"
    "x-scheme-handler/roblox-studio-auth"
    "x-scheme-handler/betterstrap"
  ];
in
{
  options.programs.betterstrap = {
    enable = lib.mkEnableOption "BetterStrap, a Roblox bootstrapper that launches Roblox through Sober";

    package = lib.mkOption {
      type = lib.types.package;
      default = self.packages.${pkgs.stdenv.hostPlatform.system}.betterstrap;
      defaultText = lib.literalExpression "betterstrap.packages.\${pkgs.stdenv.hostPlatform.system}.betterstrap";
      description = "The BetterStrap package to install.";
    };

    openRobloxLinks = lib.mkOption {
      type = lib.types.bool;
      default = false;
      description = "Open roblox:// and betterstrap:// links from the browser with BetterStrap through a Home Manager managed mimeapps.list. BetterStrap registers these links itself when this is off.";
    };
  };

  config = lib.mkIf cfg.enable {
    home.packages = [ cfg.package ];

    xdg.mimeApps = lib.mkIf cfg.openRobloxLinks {
      enable = lib.mkDefault true;
      defaultApplications = lib.genAttrs schemes (_: desktopEntry);
    };
  };
}
