"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vscode_1 = require("vscode");
const vscode = require("vscode");
const fs = require("fs");
const YAML = require("yamljs");
var edge = require('edge-js');
class TocNode {
    constructor(title, uri) {
        this.tocTitle = title ? title : "";
        this.resource = uri;
        this.isParent = false;
    }
}
exports.TocNode = TocNode;
class TocModelMd {
    constructor(rootTocFile) {
        this.rootTocFile = rootTocFile;
        // nodes contains a map from the referenced unique file path to TOC node.
        this.nodes = new Map();
        // Create the root node
        this.rootNode = new TocNode();
        this.nodes.set("", this.rootNode);
        this.openedRootToc = false;
        if (this.rootTocFile) {
            this.openToc(this.rootTocFile);
            this.openedRootToc = true;
        }
    }
    openToc(tocFile, parentNode) {
        if (!parentNode) {
            parentNode = this.rootNode;
        }
        // Only run once.
        if (this.openedRootToc) {
            return this.rootNode;
        }
        // Load the toc file from the specified path
        var buf = fs.readFileSync(tocFile.fsPath);
        // parse it into a tree data structure
        var lines = buf.toString().split("\n");
        var lineNumber = 0;
        var nestingLevel = 0;
        var currentNode = this.rootNode;
        var topLevelNode = this.rootNode;
        do {
            // skip blank lines
            if (lines[lineNumber] === "") {
                continue;
            }
            var hashCount = 0;
            while (lines[lineNumber].charAt(hashCount) === '#') {
                hashCount++;
            }
            if (hashCount === 0) {
                continue;
            }
            // Check if this will be the root node
            if (nestingLevel === 0) {
                if (hashCount !== 1) {
                    // error: single node must be at the top.
                }
                else {
                    // Initialize the node, with the old (empty) rootNode as the parent
                    var newNode = this.makeSingleNode(lines[lineNumber].substr(hashCount), tocFile, parentNode);
                    topLevelNode = newNode;
                    currentNode = newNode;
                }
            }
            else {
                if (nestingLevel === hashCount - 1) {
                    // Add the first child node
                    newNode = this.makeSingleNode(lines[lineNumber].substr(hashCount), tocFile, currentNode);
                    currentNode = newNode;
                }
                else if (nestingLevel === hashCount) {
                    // Add a sibling node
                    newNode = this.makeSingleNode(lines[lineNumber].substr(hashCount), tocFile, currentNode.parentNode);
                    currentNode = newNode;
                }
                else if (nestingLevel === hashCount + 1) {
                    // Pop up a level and add a sibling node to the parent node
                    newNode = this.makeSingleNode(lines[lineNumber].substr(hashCount), tocFile, currentNode.parentNode ? currentNode.parentNode.parentNode : undefined);
                    currentNode = newNode;
                }
                else if (nestingLevel === hashCount + 2) {
                    // Pop up a level and add a sibling node to the parent node
                    newNode = this.makeSingleNode(lines[lineNumber].substr(hashCount), tocFile, currentNode.parentNode ? currentNode.parentNode.parentNode ? currentNode.parentNode.parentNode.parentNode : undefined : undefined);
                    currentNode = newNode;
                }
                else if (nestingLevel === hashCount + 3) {
                    // Pop up a level and add a sibling node to the parent node - terrible hack, sorry everyone, need to do a recursive version.
                    newNode = this.makeSingleNode(lines[lineNumber].substr(hashCount), tocFile, currentNode.parentNode ? currentNode.parentNode.parentNode ? currentNode.parentNode.parentNode.parentNode ? currentNode.parentNode.parentNode.parentNode.parentNode : undefined : undefined : undefined);
                    currentNode = newNode;
                }
                else if (nestingLevel === hashCount + 4) {
                    // Pop up a level and add a sibling node to the parent node - terrible hack, sorry everyone, need to do a recursive version.
                    newNode = this.makeSingleNode(lines[lineNumber].substr(hashCount), tocFile, currentNode.parentNode ? currentNode.parentNode.parentNode ? currentNode.parentNode.parentNode.parentNode ? currentNode.parentNode.parentNode.parentNode.parentNode ? currentNode.parentNode.parentNode.parentNode.parentNode.parentNode : undefined : undefined : undefined : undefined);
                    currentNode = newNode;
                }
                else if (nestingLevel === hashCount + 5) {
                    // Pop up a level and add a sibling node to the parent node - terrible hack, sorry everyone, need to do a recursive version.
                    newNode = this.makeSingleNode(lines[lineNumber].substr(hashCount), tocFile, currentNode.parentNode ? currentNode.parentNode.parentNode ? currentNode.parentNode.parentNode.parentNode ? currentNode.parentNode.parentNode.parentNode.parentNode ? currentNode.parentNode.parentNode.parentNode.parentNode.parentNode ? currentNode.parentNode.parentNode.parentNode.parentNode.parentNode.parentNode : undefined : undefined : undefined : undefined : undefined);
                    currentNode = newNode;
                }
                else if (nestingLevel === hashCount + 6) {
                    // Pop up a level and add a sibling node to the parent node - terrible hack, sorry everyone, need to do a recursive version.
                    newNode = this.makeSingleNode(lines[lineNumber].substr(hashCount), tocFile, currentNode.parentNode ? currentNode.parentNode.parentNode ? currentNode.parentNode.parentNode.parentNode ? currentNode.parentNode.parentNode.parentNode.parentNode ? currentNode.parentNode.parentNode.parentNode.parentNode.parentNode ? currentNode.parentNode.parentNode.parentNode.parentNode.parentNode.parentNode ? currentNode.parentNode.parentNode.parentNode.parentNode.parentNode.parentNode : undefined : undefined : undefined : undefined : undefined : undefined);
                    currentNode = newNode;
                }
                else if (nestingLevel === hashCount + 7) {
                    // Pop up a level and add a sibling node to the parent node - terrible hack, sorry everyone, need to do a recursive version.
                    newNode = this.makeSingleNode(lines[lineNumber].substr(hashCount), tocFile, currentNode.parentNode ? currentNode.parentNode.parentNode ? currentNode.parentNode.parentNode.parentNode ? currentNode.parentNode.parentNode.parentNode.parentNode ? currentNode.parentNode.parentNode.parentNode.parentNode.parentNode ? currentNode.parentNode.parentNode.parentNode.parentNode.parentNode.parentNode ? currentNode.parentNode.parentNode.parentNode.parentNode.parentNode.parentNode ? currentNode.parentNode.parentNode.parentNode.parentNode.parentNode.parentNode.parentNode : undefined : undefined : undefined : undefined : undefined : undefined : undefined);
                    currentNode = newNode;
                }
                else {
                    // I believe it's the case that the online TOC supports a maximum of 8 levels of nesting.
                    vscode.window.showErrorMessage("Unsupported level of nesting in TOC.");
                }
            }
            nestingLevel = hashCount;
        } while (++lineNumber < lines.length);
        return topLevelNode;
    }
    makeSingleNode(textLine, tocFile, parent) {
        // parse the string as a bare title (a parent without a link)
        // or as a reference to a topic
        // Find first bracket. If not found, assume bare title.
        var newNode;
        let firstBracket = textLine.indexOf("[");
        if (firstBracket === -1) {
            // Not found [], must be a regular node, without a link.
            newNode = new TocNode(textLine);
        }
        else {
            // Parse a Markdown link
            var basePath = tocFile ? tocFile.fsPath.substring(0, tocFile.fsPath.lastIndexOf("\\") + 1) : ".";
            var closingBracket = textLine.indexOf("]");
            var titleString = textLine.substr(firstBracket + 1, closingBracket - 2);
            textLine = textLine.substr(closingBracket + 1); // get remaining part of the string
            var firstParen = textLine.indexOf("(");
            var closingParen = textLine.indexOf(")");
            var linkText = textLine.substr(firstParen + 1, closingParen - 1);
            linkText = basePath + linkText;
            newNode = new TocNode(titleString, vscode.Uri.file(linkText));
            this.nodes.set(linkText, newNode);
            if (linkText.toLowerCase().includes("toc.md")) {
                this.openToc(vscode.Uri.file(linkText), newNode);
            }
        }
        if (parent) {
            if (parent.children) {
                // Add the new node to the children list of the parent node.
                parent.children.push(newNode);
            }
            else {
                // Create a parent node array and add the new node.
                parent.children = new Array();
                parent.children.push(newNode);
                parent.isParent = true;
            }
            // Set the parentNode of the current node to the parent
            newNode.parentNode = parent;
        }
        return newNode;
    }
    connect(tocUri) {
        return new Promise((tocModel, e) => {
            // Using the given tocFile
            // Try to open it
            // If it fails, error out
            // If it succeeds, load it.
            //Not sure what the template parameter should be here.
            if (this.openToc(tocUri)) {
                tocModel(this);
            }
            else {
                e("Failed to open TOC at path " + tocUri.fsPath);
            }
        });
    }
    get roots() {
        if (this.rootTocFile) {
            return this.connect(this.rootTocFile).then(toc => {
                return new Promise((c, e) => {
                    // generate the list of the root nodes at the top level
                    // Open the file specified by "uri"
                    if (toc.rootNode.children) {
                        c(toc.rootNode.children);
                    }
                    else {
                        e("The root node is empty.");
                    }
                });
            });
        }
        else {
            return new Promise((c, e) => {
                e("No TOC loaded.");
            });
        }
    }
    getChildren(node) {
        return new Promise((tocNodes, e) => {
            // given the node get its children
            if (node.children) {
                tocNodes(node.children);
            }
            else {
                e("There are no children for the node " + node.tocTitle);
            }
        });
    }
    getContent(resource) {
        return new Promise((content, e) => {
            // Get the content (markdown file contents) for this node
            fs.readFile(resource.fsPath, (err, data) => {
                if (err) {
                    e(err);
                }
                else {
                    content(data.toString());
                }
            });
        });
    }
    getNode(resource) {
        return this.nodes.get(resource.fsPath);
    }
}
exports.TocModelMd = TocModelMd;
class TocItem {
    constructor(name, href, expanded, items) {
        this.name = name;
        this.href = href;
        this.expanded = expanded;
        this.items = items;
    }
}
class TocModelYaml {
    constructor(tocFile) {
        this.tocFile = tocFile;
        // nodes contains a map from the referenced unique file path to TOC node.
        this.nodes = new Map();
        // Create the root node
        this.rootNode = new TocNode();
        this.nodes.set("", this.rootNode);
        this.openedToc = false;
        if (tocFile) {
            this.openToc(tocFile);
        }
    }
    recurseNodes(tocyaml, parent) {
        tocyaml.forEach((item, index) => {
            var uri = undefined;
            // Get URI
            if (item.href) {
                if (this.tocFile) {
                    var folder = this.tocFile.fsPath.substring(0, this.tocFile.fsPath.lastIndexOf("\\") + 1);
                    var fullPath = folder + item.href;
                    uri = vscode_1.Uri.file(fullPath);
                }
            }
            // Get TocNode
            var node = new TocNode(item.name, uri);
            node.parentNode = parent;
            node.parentNode.isParent = true;
            // Add to children array
            if (!node.parentNode.children) {
                node.parentNode.children = new Array();
                // workaround an apparent bug.
                while (node.parentNode.children.length > 0) {
                    node.parentNode.children.pop();
                }
            }
            node.parentNode.children.push(node);
            if (uri) {
                this.nodes.set(uri.fsPath, node);
            }
            if (item.items) {
                this.recurseNodes(item.items, node);
            }
        });
    }
    openToc(tocFile) {
        // Only run once.
        if (this.openedToc) {
            return this;
        }
        // Load the toc file from the specified path
        var buf = fs.readFileSync(tocFile.fsPath);
        var tocyaml = YAML.load(tocFile.fsPath);
        this.recurseNodes(tocyaml, this.rootNode);
        this.openedToc = true;
        return this;
    }
    connect(tocUri) {
        return new Promise((tocModel, e) => {
            // Using the given tocFile
            // Try to open it
            // If it fails, error out
            // If it succeeds, load it.
            //Not sure what the template parameter should be here.
            if (this.openToc(tocUri)) {
                tocModel(this);
            }
            else {
                e("Failed to open TOC at path " + tocUri.fsPath);
            }
        });
    }
    get roots() {
        if (this.tocFile) {
            return this.connect(this.tocFile).then(toc => {
                return new Promise((c, e) => {
                    // generate the list of the root nodes at the top level
                    // Open the file specified by "uri"
                    if (toc.rootNode.children) {
                        c(toc.rootNode.children);
                    }
                    else {
                        e("The root node is empty.");
                    }
                });
            });
        }
        else {
            return new Promise((c, e) => {
                e("No TOC loaded.");
            });
        }
    }
    getChildren(node) {
        return new Promise((tocNodes, e) => {
            // given the node get its children
            if (node.children) {
                tocNodes(node.children);
            }
            else {
                e("There are no children for the node " + node.tocTitle);
            }
        });
    }
    getContent(resource) {
        return new Promise((content, e) => {
            // Get the content (markdown file contents) for this node
            fs.readFile(resource.fsPath, (err, data) => {
                if (err) {
                    e(err);
                }
                else {
                    content(data.toString());
                }
            });
        });
    }
    getNode(resource) {
        return this.nodes.get(resource.fsPath);
    }
}
exports.TocModelYaml = TocModelYaml;
class TocTreeDataProvider {
    constructor(model) {
        this.model = model;
        this._onDidChangeTreeData = new vscode_1.EventEmitter();
        this.onDidChangeTreeData = this._onDidChangeTreeData.event;
    }
    refresh() {
        this._onDidChangeTreeData.fire();
    }
    getTreeItem(element) {
        return {
            label: element.tocTitle,
            collapsibleState: element.isParent ? vscode_1.TreeItemCollapsibleState.Collapsed : void 0,
            command: element.isParent ? void 0 : {
                command: 'tocExplorer.openResource',
                arguments: [element.resource],
                title: 'Open TOC Resource'
            }
        };
    }
    getChildren(element) {
        return element ? this.model.getChildren(element) : this.model.roots;
    }
    getParent(element) {
        return element.parentNode;
    }
    provideTextDocumentContent(uri, token) {
        return this.model.getContent(uri).then(content => content);
    }
}
exports.TocTreeDataProvider = TocTreeDataProvider;
/*class SearchResult {

    constructor(readonly Content: string, readonly Filename: string, readonly Title: string) {}
}*/
class TocExplorer {
    listener(event) {
        if (event && event.document.languageId === "markdown") {
            this.openToc(event.document);
        }
    }
    openToc(doc) {
        var isYamlToc = false;
        vscode.window.showInformationMessage("Opening TOC");
        // show TOC for that file
        var fileName = doc.fileName;
        // Get the path part 
        var folder = fileName.substring(0, fileName.lastIndexOf("\\") + 1);
        // Find a TOC at this path
        if (fs.existsSync(folder + "toc.md")) {
            this.tocPathOrUndefined = folder + "toc.md";
            isYamlToc = false;
        }
        if (fs.existsSync(folder + "toc.yml")) {
            this.tocPathOrUndefined = folder + "toc.yml";
            isYamlToc = true;
        }
        if (this.tocPathOrUndefined) {
            var tocPath = this.tocPathOrUndefined;
            if (this.tocModelOrUndefined) {
                // Is this TOC already open?
                if (this.tocModelOrUndefined.tocFile && this.tocModelOrUndefined.tocFile.fsPath === tocPath) {
                    // TODO: activate the node for this file
                    var displayThisNode = this.tocModelOrUndefined.getNode(vscode.Uri.file(doc.fileName));
                    if (displayThisNode) {
                        if (this.tocViewerOrUndefined) {
                            this.tocViewerOrUndefined.reveal(displayThisNode);
                        }
                    }
                    return;
                }
            }
            if (isYamlToc) {
                this.tocModelOrUndefined = new TocModelYaml(vscode.Uri.file(tocPath));
            }
            else {
                this.tocModelOrUndefined = new TocModelMd(vscode.Uri.file(tocPath));
            }
            var tocModel = this.tocModelOrUndefined;
            const treeDataProvider = new TocTreeDataProvider(tocModel);
            this.context.subscriptions.push(vscode.workspace.registerTextDocumentContentProvider('toc', treeDataProvider));
            this.tocViewerOrUndefined = vscode.window.createTreeView('tocExplorer', { treeDataProvider });
            if (this.subscriptionRefresh) {
                this.subscriptionRefresh.dispose();
            }
            if (this.subscriptionOpenResource) {
                this.subscriptionOpenResource.dispose();
            }
            if (this.subscriptionRevealResource) {
                this.subscriptionRevealResource.dispose();
            }
            this.subscriptionRefresh = vscode.commands.registerCommand('tocExplorer.refresh', () => treeDataProvider.refresh());
            this.subscriptionOpenResource = vscode.commands.registerCommand('tocExplorer.openResource', resource => this.openResource(resource));
            this.subscriptionRevealResource = vscode.commands.registerCommand('tocExplorer.revealResource', () => this.reveal());
            this.reveal();
        }
    }
    constructor(context) {
        this.context = context;
        if (vscode.window.activeTextEditor) {
            this.openToc(vscode.window.activeTextEditor.document);
        }
        // start listening
        this.subscription = vscode.window.onDidChangeActiveTextEditor(this.listener, this);
        // test edge connection
        this.testEdgeConnection();
    }
    testEdgeConnection() {
        // shall we?
        //var edge = require('edge');
        var helloWorld = edge.func(function () {
        });
        // var searchMethod = edge.func('../RepoManager/LuceneSearch/bin/Debug/netstandard2.0/LucerneSearch.dll');
        //var results : SearchResult[] = searchMethod("test");
        // results.forEach( (item) => { console.log(item.Content + " " + item.Filename + " " + item.Title)});
    }
    openResource(resource) {
        vscode.workspace.openTextDocument(resource).then(document => {
            this.openToc(document);
        });
        vscode.commands.executeCommand('markdown.showPreview', resource);
    }
    reveal() {
        const node = this.getNode();
        if (node) {
            if (this.tocViewerOrUndefined) {
                return this.tocViewerOrUndefined.reveal(node);
            }
        }
        return undefined;
    }
    getNode() {
        if (vscode.window.activeTextEditor) {
            if (this.tocModelOrUndefined) {
                var node = this.tocModelOrUndefined.getNode(vscode.window.activeTextEditor.document.uri);
                return node;
            }
        }
        return undefined;
    }
    dispose() {
        this.subscription.dispose();
        if (this.subscriptionOpenResource) {
            this.subscriptionOpenResource.dispose();
        }
        if (this.subscriptionRefresh) {
            this.subscriptionRefresh.dispose();
        }
        if (this.subscriptionRevealResource) {
            this.subscriptionRevealResource.dispose();
        }
    }
}
exports.TocExplorer = TocExplorer;
//# sourceMappingURL=tocExplorer.textDocumentContentProvider.js.map