const {readFile, writeFile} = require('fs').promises;

const inputFile = process.argv[2];
const outputFile = process.argv[3];
replace().then(() => console.log('Finished.'));

async function replace() {
    
    let openApiDefinitionJson = await readFile(inputFile, 'utf8');
    const openApiDefinition = JSON.parse(openApiDefinitionJson);
    for (const [path, entries] of Object.entries(openApiDefinition.paths)) {
        const operations = Object.entries(entries).filter(([key, ]) => key.match(/^(get|put|post|delete)$/));
        for (const [operationName, operation] of operations) {
            operation['x-csharp-action'] = humaziePath(path, operationName);
        }
    }

    openApiDefinitionJson = JSON.stringify(openApiDefinition, null, '  ');
    await writeFile(outputFile, openApiDefinitionJson, 'utf8');
}

function humaziePath(path, operation) {
    if (!path)
        return path;

    var parts = path
        .split('/')
        .filter(part => part && part != '{realm}')

    var pathParts = parts
        .filter(part => part[0] != '{')
        .map(part => humaziePathPart(part))

    var parameterParts = parts
        .filter(part => part[0] == '{')
        .map(part => part.replace(/[\{\}]/g, ''))
        .map(part => humaziePathPart(part));

    operation = uppercaseFirstChar(operation);
    let camlCasedPath = `${operation}${pathParts.join('')}`;
    if (parameterParts.length)
        camlCasedPath = camlCasedPath + `By${parameterParts.join('And')}`;

    return camlCasedPath;
}

function humaziePathPart(pathPart) {
    var parts = pathPart
        .split(/[-_]/)
        .map(part => uppercaseFirstChar(part));
    return parts.join('');
}

function uppercaseFirstChar(value){
    if (!value)
        return value;
    return value[0].toUpperCase() + value.slice(1);
}