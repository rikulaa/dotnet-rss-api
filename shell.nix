{ pkgs ? import (fetchTarball "https://github.com/NixOS/nixpkgs/archive/21808d22b1cda1898b71cf1a1beb524a97add2c4.tar.gz") {} }:

with pkgs;
let
  inherit (lib) optional optionals;
in
 pkgs.mkShell {
    # nativeBuildInputs is usually what you want -- tools you need to run
    # elixir v1.15.7
    # postgres 13.4
    nativeBuildInputs = [ pkgs.omnisharp-roslyn ]
    ++ optional stdenv.isLinux inotify-tools # For file_system on Linux.
    ++ optionals stdenv.isDarwin (with darwin.apple_sdk.frameworks; [
      # For file_system on macOS.
      CoreFoundation
      CoreServices
    ]);
}
