const fs = require("fs");
const path = require("path");

const OSU_LIBRARY = [
    "osu.Game.dll",
    "osu.Game.Rulesets.Osu.dll",
    "osu.Game.Rulesets.Taiko.dll",
    "osu.Game.Rulesets.Catch.dll",
    "osu.Game.Rulesets.Mania.dll",
    "osu.Framework.dll",
    "MessagePack.dll",
    "MessagePack.Annotations.dll",
    "AutoMapper.dll",
    "osuTK.dll",
    "Realm.dll",
    "Newtonsoft.Json.dll",
    "nunit.framework.dll",
    "ppy.ManagedBass.dll"
];

const SOFTWARE_LIBRARY = [
    "MongoDB.Bson.dll",
    "LiteDB.dll",
    "OsuMemoryDataProvider.dll",
    "ProcessMemoryDataFinder.dll",
    "OsuParsers.dll"
];

const REALTIMEPPUR_FILES = [
    "osu!private.dll",
    "osu!private.dll.config",
    "osu!private.exe",
    "osu!private.runtimeconfig.json"
];

const BUILD_FOLDER = "../../build";

if (!fs.existsSync(BUILD_FOLDER)) {
    fs.mkdirSync(BUILD_FOLDER);
} else {
    const BUILD_FOLDER_CONTENTS = fs.readdirSync(BUILD_FOLDER);
    for (const file of BUILD_FOLDER_CONTENTS) {
        const filePath = path.join(BUILD_FOLDER, file);
        console.log(`Deleting ${filePath}`);
        const buildFileStat = fs.statSync(filePath);
        if (buildFileStat.isDirectory()) {
            fs.rmSync(filePath, { recursive: true, force: true });
        } else {
            fs.unlinkSync(filePath);
        }
    }
}

for (const file of OSU_LIBRARY) {
    const filePath = path.join(BUILD_FOLDER, file);
    console.log(`Copying ${file} to ${filePath}`);
    fs.copyFileSync(file, filePath);
    fs.unlinkSync(file);
}

for (const file of SOFTWARE_LIBRARY) {
    const filePath = path.join(BUILD_FOLDER, file);
    console.log(`Copying ${file} to ${filePath}`);
    fs.copyFileSync(file, filePath);
    fs.unlinkSync(file);
}

for (const file of REALTIMEPPUR_FILES) {
    const filePath = path.join(BUILD_FOLDER, file);
    console.log(`Copying ${file} to ${filePath}`);
    fs.copyFileSync(file, filePath);
    fs.unlinkSync(file);
}

const fileList = fs.readdirSync(".");
for (const file of fileList) {
    const filePath = path.join(".", file);
    const stat = fs.statSync(filePath);

    if (stat.isDirectory()) {
        console.log(`Deleting folder ${filePath}`);
        fs.rmSync(filePath, { recursive: true, force: true });
    } else {
        console.log(`Deleting file ${filePath}`);
        fs.unlinkSync(filePath);
    }
}

console.log("Build completed.");
