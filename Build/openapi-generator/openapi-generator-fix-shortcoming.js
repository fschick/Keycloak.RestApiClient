const {resolve} = require('path');
const {readdir, readFile, writeFile} = require('fs').promises;

const projectImports = `
  <Import Project="../../../Build/targets/net_standard.props" />
  <Import Project="../../../Build/targets/version.props" />
  <Import Project="../../../Build/targets/nuget.props" />

  <ItemGroup>
    <None Include="../../../FS.Keycloak.RestApiClient.png" Pack="true" PackagePath="Schick.Keycloak.RestApiClient.png"/>
    <None Include="../../README.md" Pack="true" PackagePath="README.md" />
  </ItemGroup>
`;

const projectFixes = [
	{search: /<PropertyGroup>.*<\/PropertyGroup>/s, replace: ''},
	{search: /<ItemGroup>((?!PackageReference).)*?<\/ItemGroup>/sg, replace: ''},
	{search: /<Project Sdk="Microsoft.NET.Sdk">/g, replace: `<Project Sdk="Microsoft.NET.Sdk">${projectImports}`},
];

const fixes = [
	// Sample:
	// {search: /cc/g, replace: 'ee'},
	{search: /(DateTimeOffset)\?\?/g, replace: '$1?'},
];

console.log('Replace version information ...');

const directory = process.argv[2];
replaceInFile('FS.Keycloak.RestApiClient/src/FS.Keycloak.RestApiClient/FS.Keycloak.RestApiClient.csproj', projectFixes)
	.then(() => {
		console.log('Replace in directory ', directory);
		return replaceInDirectory(directory, fixes);
	})
	.then(() => console.log('finished'));


async function replaceInDirectory(directory, replacements) {
    for await (const file of getFiles(directory))
        await replaceInFile(file, replacements);
}

async function* getFiles(directory) {
    const dirEntries = await readdir(directory, {withFileTypes: true});
    for (const entry of dirEntries) {
        const entryName = resolve(directory, entry.name);
        if (entry.isDirectory())
            yield* getFiles(entryName);
        else
            yield entryName;
    }
}

async function replaceInFile(file, replacements) {
	console.log('Replace for file:', file);
    const fileContent = await readFile(file, 'utf8');

    let replacedContent = fileContent;
    for (const replacement of replacements)
        replacedContent = replacedContent.replace(replacement.search, replacement.replace);

    if (replacedContent !== fileContent)
        await writeFile(file, replacedContent, 'utf8');
}