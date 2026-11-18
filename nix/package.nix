{
  lib,
  stdenv,
  buildDotnetModule,
  dotnetCorePackages,
  fetchFromGitHub,
  rustPlatform,
  autoPatchelfHook,
  nix-update-script,
  alsa-lib,
  dbus,
  fontconfig,
  freetype,
  glib,
  gst_all_1,
  icu,
  libGL,
  libICE,
  libSM,
  libX11,
  libXcomposite,
  libXcursor,
  libXdamage,
  libXext,
  libXfixes,
  libXi,
  libXrandr,
  libXrender,
  libxkbcommon,
  libdrm,
  mesa,
  openssl,
  vulkan-loader,
  wayland,
  zlib,
}:

let
  runtimeLibraries = [
    alsa-lib
    dbus
    fontconfig
    freetype
    glib
    gst_all_1.gstreamer
    gst_all_1.gst-plugins-base
    icu
    libGL
    libICE
    libSM
    libX11
    libXcomposite
    libXcursor
    libXdamage
    libXext
    libXfixes
    libXi
    libXrandr
    libXrender
    libxkbcommon
    libdrm
    mesa
    openssl
    vulkan-loader
    wayland
    zlib
  ];
in
buildDotnetModule (finalAttrs: {
  pname = "betterstrap";
  version = "1.1.3.7";

  src = fetchFromGitHub {
    owner = "KloBraticc";
    repo = "BetterStrap";
    tag = "v${finalAttrs.version}";
    fetchSubmodules = true;
    hash = "sha256-YQvMuL//uUuFKGaOMfuDWNHAfnHx1s0BjgzGfVLweug=";
  };

  projectFile = "src/BetterStrap.Cross/BetterStrap.Cross.csproj";
  nugetDeps = ./deps.json;

  dotnet-sdk = dotnetCorePackages.sdk_10_0;
  dotnet-runtime = null;
  selfContainedBuild = true;

  executables = [ "BetterStrap" ];

  nativeBuildInputs = [ autoPatchelfHook ];
  buildInputs = [ (lib.getLib stdenv.cc.cc) ] ++ runtimeLibraries;
  runtimeDeps = runtimeLibraries;
  autoPatchelfIgnoreMissingDeps = [ "*" ];

  preConfigure = ''
    dotnetFlags+=("-p:BetterStrapLinuxPackagingRoot=$PWD/.nix-artifacts/")
  '';

  dotnetBuildFlags = [ "-p:VoidImageLibrary=${finalAttrs.passthru.voidimage}/lib/libvoidimage.so" ];
  dotnetInstallFlags = [
    "-p:PublishSingleFile=false"
    "-p:IncludeAllContentForSelfExtract=false"
    "-p:DebugType=None"
    "-p:DebugSymbols=false"
    "-p:VoidImageLibrary=${finalAttrs.passthru.voidimage}/lib/libvoidimage.so"
  ];

  env = {
    BETTERSTRAP_VOIDIMAGE_NO_ZIG = "1";
    BETTERSTRAP_EDITOR_COMPILER = "${stdenv.cc}/bin/cc";
  };

  postInstall = ''
    for library in uc i18n data; do
      if [ -e $out/lib/betterstrap/libicu$library.so.72.1.0.3 ]; then
        ln -s libicu$library.so.72.1.0.3 $out/lib/betterstrap/libicu$library.so.72
      fi
    done
    install -Dm644 build/Packaging/Linux/betterstrap.desktop \
      $out/share/applications/io.github.KloBraticc.BetterStrap.desktop
    substituteInPlace $out/share/applications/io.github.KloBraticc.BetterStrap.desktop \
      --replace-fail "Exec=betterstrap" "Exec=$out/bin/betterstrap"
    install -Dm644 src/BetterStrap.App/BetterStrap.png \
      $out/share/icons/hicolor/256x256/apps/io.github.KloBraticc.BetterStrap.png
    install -Dm644 build/Packaging/Linux/io.github.KloBraticc.BetterStrap.metainfo.xml \
      $out/share/metainfo/io.github.KloBraticc.BetterStrap.metainfo.xml
  '';

  postFixup = ''
    ln -s BetterStrap $out/bin/betterstrap
  '';

  passthru = {
    voidimage = rustPlatform.buildRustPackage {
      pname = "voidimage";
      inherit (finalAttrs) version src;
      cargoRoot = "native/voidimage";
      buildAndTestSubdir = "native/voidimage";
      cargoLock.lockFile = "${finalAttrs.src}/native/voidimage/Cargo.lock";
      doCheck = false;
    };
    updateScript = nix-update-script { };
  };

  meta = {
    description = "Roblox bootstrapper that launches Roblox through Sober on Linux";
    homepage = "https://github.com/KloBraticc/BetterStrap";
    changelog = "https://github.com/KloBraticc/BetterStrap/releases/tag/v${finalAttrs.version}";
    license = lib.licenses.mit;
    mainProgram = "betterstrap";
    platforms = [
      "x86_64-linux"
      "aarch64-linux"
    ];
  };
})
