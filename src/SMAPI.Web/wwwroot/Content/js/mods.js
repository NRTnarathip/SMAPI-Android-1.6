var smapi = smapi || {};
var app;
smapi.modList = async function (state) {
    // init data
    const defaultStats = {
        total: 0,
        compatible: 0,
        workaround: 0,
        soon: 0,
        broken: 0,
        abandoned: 0,
        invalid: 0,
        percentCompatible: 0,
        percentBroken: 0,
        percentObsolete: 0
    };
    const data = {
        mods: [],
        showAdvanced: false,
        visibleMainStats: $.extend({}, defaultStats),
        filters: {
            source: {
                value: {
                    open: { value: true },
                    closed: { value: true }
                }
            },
            status: {
                label: "status",
                value: {
                    // note: keys must match status returned by the API
                    ok: { value: true },
                    optional: { value: true },
                    unofficial: { value: true },
                    workaround: { value: true },
                    broken: { value: true },
                    abandoned: { value: true },
                    obsolete: { value: true }
                }
            },
            download: {
                value: {
                    chucklefish: { value: true, label: "Chucklefish" },
                    curseforge: { value: true, label: "CurseForge" },
                    moddrop: { value: true, label: "ModDrop" },
                    nexus: { value: true, label: "Nexus" },
                    custom: { value: true }
                }
            }
        },
        search: ""
    };

    // fetch data
    if (state.fetchUri)
        data.mods = await $.getJSON(state.fetchUri);
    else
        data.mods = state.fetchedData ?? [];

    // init filters
    Object.entries(data.filters).forEach(([groupKey, filterGroup]) => {
        filterGroup.label = filterGroup.label || groupKey;
        Object.entries(filterGroup.value).forEach(([filterKey, filter]) => {
            filter.id = ("filter_" + groupKey + "_" + filterKey).replace(/[^a-zA-Z0-9]/g, "_");
            filter.label = filter.label || filterKey;
        });
    });

    // init mods
    for (let i = 0; i < data.mods.length; i++) {
        const mod = data.mods[i];

        // set initial visibility
        mod.Visible = true;

        // set default compatibility
        if (!mod.Compatibility)
            mod.Compatibility = { "Status": "ok", "Summary": "use latest version." };

        // concatenate searchable text
        mod.SearchableText = [mod.Name, mod.AlternateNames, mod.Author, mod.AlternateAuthors, mod.Compatibility.Summary, mod.BrokeIn];
        if (mod.Compatibility.UnofficialVersion)
            mod.SearchableText.push(mod.Compatibility.UnofficialVersion);
        mod.SearchableText = mod.SearchableText.join(" ").toLowerCase();
    }

    // init app
    const methods = {
        /**
         * Update the visibility of all mods based on the current search text and filters.
         */
        applyFilters: function () {
            // get search terms
            const words = data.search.toLowerCase().split(" ");

            // apply criteria
            const stats = data.visibleMainStats = $.extend({}, defaultStats);
            for (let i = 0; i < data.mods.length; i++) {
                const mod = data.mods[i];
                mod.Visible = true;

                // check filters
                mod.Visible = this.matchesFilters(mod, words);
                if (mod.Visible) {
                    stats.total++;
                    stats[this.getCompatibilityGroup(mod.Compatibility.Status, !!mod.SourceUrl)]++;
                }
            }

            // add aggregate stats
            stats.percentCompatible = Math.round((stats.compatible + stats.workaround) / stats.total * 100);
            stats.percentBroken = Math.round((stats.soon + stats.broken) / stats.total * 100);
            stats.percentObsolete = Math.round(stats.abandoned / stats.total * 100);
        },

        /**
         * Fix the window position for the current hash.
         */
        fixHashPosition: function () {
            const anchor = location.hash.substring(1);
            if (!anchor)
                return;

            // jump to exact anchor
            if (methods.scrollToRow(anchor))
                return;

            // else jump to mod by ID
            for (let i = 0; i < data.mods.length; i++) {
                const mod = data.mods[i];

                for (let n = 0; n < mod.Id.length; n++) {
                    if (anchor.localeCompare(mod.Id[n], undefined, { sensitivity: 'accent' }) === 0) {
                        if (methods.scrollToRow(mod.Slug))
                            return;
                    }
                }
            }
        },

        /**
         * Scroll the page so the given table row comes into view.
         * @param id The ID of the row element to match.
         * @returns {boolean} Whether the target was found and scrolled into view.
         */
        scrollToRow: function(id) {
            const row = $("#" + id);
            const target = row.prev().get(0) || row.get(0); // show previous row if possible, to push the actual row below the sticky header
            target?.scrollIntoView();
            return !!target;
        },

        /**
         * Get whether a mod matches the current filters.
         * @param {object} mod The mod to check.
         * @param {string[]} searchWords The search words to match.
         * @returns {boolean} Whether the mod matches the filters.
         */
        matchesFilters: function (mod, searchWords) {
            const filters = data.filters;

            // check hash
            if (location.hash === "#" + mod.Slug)
                return true;

            // check source
            if (!filters.source.value.open.value && mod.SourceUrl)
                return false;
            if (!filters.source.value.closed.value && !mod.SourceUrl)
                return false;

            // check status
            const mainStatus = mod.Compatibility.Status;
            if (filters.status.value[mainStatus] && !filters.status.value[mainStatus].value)
                return false;

            // check download sites
            const ignoreSites = [];

            if (!filters.download.value.chucklefish.value)
                ignoreSites.push("Chucklefish");
            if (!filters.download.value.curseforge.value)
                ignoreSites.push("CurseForge");
            if (!filters.download.value.moddrop.value)
                ignoreSites.push("ModDrop");
            if (!filters.download.value.nexus.value)
                ignoreSites.push("Nexus");
            if (!filters.download.value.custom.value)
                ignoreSites.push("custom");

            if (ignoreSites.length) {
                let anyLeft = false;
                for (let i = 0; i < mod.ModPages.length; i++) {
                    if (ignoreSites.indexOf(mod.ModPages[i].Text) === -1) {
                        anyLeft = true;
                        break;
                    }
                }

                if (!anyLeft)
                    return false;
            }

            // check search terms
            for (let w = 0; w < searchWords.length; w++) {
                if (mod.SearchableText.indexOf(searchWords[w]) === -1)
                    return false;
            }

            return true;
        },

        /**
         * Get a mod's compatibility group for mod metrics.
         * @param {string} status The mod status for which to get the group.
         * @param {boolean} hasSourceUrl Whether the mod has a source code URL.
         * @returns {string} The compatibility group (one of 'compatible', 'workaround', 'soon', 'broken', 'abandoned', or 'invalid').
         */
        getCompatibilityGroup: function (status, hasSourceUrl) {
            switch (status) {
                // obsolete
                case "abandoned":
                case "obsolete":
                    return "abandoned";

                // compatible
                case "ok":
                case "optional":
                    return "compatible";

                // workaround
                case "workaround":
                case "unofficial":
                    return "workaround";

                // soon/broken
                case "broken":
                    if (hasSourceUrl)
                        return "soon";
                    else
                        return "broken";

                default:
                    return "invalid";
            }
        }
    };

    app = new Vue({
        el: "#app",
        data: data,
        mounted: function () {
            // enable table sorting
            $("#mod-list").tablesorter({
                cssHeader: "header",
                cssAsc: "headerSortUp",
                cssDesc: "headerSortDown"
            });

            // put focus in textbox for quick search
            if (!location.hash)
                $("#search-box").focus();

            // jump to anchor (since table is added after page load)
            this.fixHashPosition();
        },
        methods: methods
    });
    app.applyFilters();
    app.fixHashPosition();
    window.addEventListener("hashchange", function () {
        app.applyFilters();
        app.fixHashPosition();
    });
};
