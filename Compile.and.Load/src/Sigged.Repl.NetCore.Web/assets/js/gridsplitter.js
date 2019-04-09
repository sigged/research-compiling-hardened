/**
 * GridSplitter 
 * @copyright SpruceBit 2019
 * @license MIT
 */

let gridSplitter = (function () {

    return { init };

    /**
     * Initialize a gridsplitter
     */
    function init() {

        let dragging = function (event) {
            //console.log("move");
            for (let i = 0; i < _gridsplittergroups.length; i++) {
                let group = _gridsplittergroups[i];
                if (group.dragInfo.isDragging) {

                    group.grid.style.userSelect = "none";

                    if (event.type === "touchmove") {
                        group.dragInfo.currentX = event.touches[0].clientX;
                        group.dragInfo.currentY = event.touches[0].clientY;
                    } else {
                        group.dragInfo.currentX = event.clientX;
                        group.dragInfo.currentY = event.clientY;
                    }

                    //console.log("draggin'", group.dragInfo);

                    if (group.orientation === "horizontal") {
                        let newPaneColSizes = Array.from(group.dragInfo.initialPaneColSizes);
                        newPaneColSizes[group.columnIndices[0]] =
                            group.dragInfo.initialPaneColSizes[group.columnIndices[0]] - (group.dragInfo.initialX - group.dragInfo.currentX);
                        newPaneColSizes[group.columnIndices[1]] =
                            group.dragInfo.initialPaneColSizes[group.columnIndices[1]] + (group.dragInfo.initialX - group.dragInfo.currentX);
                        group.grid.style.gridTemplateColumns = newPaneColSizes.join('px ') + 'px';

                    } else if (group.orientation === "vertical") {
                        let newPaneRowSizes = Array.from(group.dragInfo.initialPaneRowSizes);
                        newPaneRowSizes[group.rowIndices[0]] =
                            group.dragInfo.initialPaneRowSizes[group.rowIndices[0]] - (group.dragInfo.initialY - group.dragInfo.currentY);
                        newPaneRowSizes[group.rowIndices[1]] =
                            group.dragInfo.initialPaneRowSizes[group.rowIndices[1]] + (group.dragInfo.initialY - group.dragInfo.currentY);
                        group.grid.style.gridTemplateRows = newPaneRowSizes.join('px ') + 'px';
                    }

                }
            }
        };
        
        // gets an array of all column/row templates in pixels
        let calcTemplateData = function (grid_w_or_h_px, grid_originalTemplateColOrRow)
        {
            let templateInPixels = new Array(grid_originalTemplateColOrRow.length);
            let fr_colrow_sum = 0.0;    //total size of col/row defined in fr
            let pixel_colrow_sum = 0.0; //total size of col/row defined in px

            for (let i_col_or_row = 0; i_col_or_row < templateInPixels.length; i_col_or_row++) {
                if (grid_originalTemplateColOrRow[i_col_or_row].endsWith("px")) {
                    let colrow_px = parseFloat(grid_originalTemplateColOrRow[i_col_or_row].replace("px", "").replace(" ", ""));
                    pixel_colrow_sum += colrow_px;
                    templateInPixels[i_col_or_row] = colrow_px;
                }
                else if (grid_originalTemplateColOrRow[i_col_or_row].endsWith("fr")) {
                    let colrow_fr = parseFloat(grid_originalTemplateColOrRow[i_col_or_row].replace("fr", "").replace(" ", ""));
                    fr_colrow_sum += colrow_fr;
                }
            }

            let px_per_one_fr = (grid_w_or_h_px - pixel_colrow_sum) / fr_colrow_sum;

            for (let i_col_or_row = 0; i_col_or_row < templateInPixels.length; i_col_or_row++) {
                if (grid_originalTemplateColOrRow[i_col_or_row].endsWith("fr")) {
                    let colrow_fr = parseFloat(grid_originalTemplateColOrRow[i_col_or_row].replace("fr", "").replace(" ", ""));
                    let col_or_row_px = colrow_fr * px_per_one_fr;
                    templateInPixels[i_col_or_row] = col_or_row_px;
                }
            }

            return {
                pixels_per_fr: px_per_one_fr,
                fr_colrow_sum,
                pixel_colrow_sum,
                templateInPixels
            };
        };


        let calculateFracFromPixels = function (grid_w_or_h_px, difference_px, grid_originalTemplateColOrRow, indicesColsOrRows)
        {
            let templateDate = calcTemplateData(grid_w_or_h_px, grid_originalTemplateColOrRow);
            grid_w_or_h_px -= templateDate.pixel_colrow_sum;

            let difference_fr = difference_px / templateDate.pixels_per_fr;
            
            if (grid_originalTemplateColOrRow[indicesColsOrRows[0]].endsWith("fr"))
            {
                let col_or_row_0_fr = parseFloat(grid_originalTemplateColOrRow[indicesColsOrRows[0]].replace("fr", "").replace(" ", ""));
                col_or_row_0_fr += difference_fr;
                grid_originalTemplateColOrRow[indicesColsOrRows[0]] = col_or_row_0_fr + "fr";
            }
            else if (grid_originalTemplateColOrRow[indicesColsOrRows[0]].endsWith("px"))
            {
                let col_or_row_0_px = parseFloat(grid_originalTemplateColOrRow[indicesColsOrRows[0]].replace("px", "").replace(" ", ""));
                col_or_row_0_px += difference_px;
                grid_originalTemplateColOrRow[indicesColsOrRows[0]] = col_or_row_0_px + "px";

            }
            if (grid_originalTemplateColOrRow[indicesColsOrRows[1]].endsWith("fr"))
            {
                let col_or_row_1_fr = parseFloat(grid_originalTemplateColOrRow[indicesColsOrRows[1]].replace("fr", "").replace(" ", ""));
                col_or_row_1_fr -= difference_fr;
                grid_originalTemplateColOrRow[indicesColsOrRows[1]] = col_or_row_1_fr + "fr";
            }
            else if (grid_originalTemplateColOrRow[indicesColsOrRows[1]].endsWith("px"))
            {
                let col_or_row_1_px = parseFloat(grid_originalTemplateColOrRow[indicesColsOrRows[1]].replace("px", "").replace(" ", ""));
                col_or_row_1_px -= difference_px;
                grid_originalTemplateColOrRow[indicesColsOrRows[1]] = col_or_row_1_px + "px";

            }
            return grid_originalTemplateColOrRow.join(" ");
        };

        let dragStop = function (event) {
            //console.log("Stop");
            document.removeEventListener("mousemove", dragging);
            document.removeEventListener("mouseup", dragStop);
            document.removeEventListener("touchmove", dragging);
            document.removeEventListener("touchend", dragStop);

            let draggersStopped = 0;
            for (let i = 0; i < _gridsplittergroups.length; i++) {
                let group = _gridsplittergroups[i];
                if (group.dragInfo.isDragging) {
                    draggersStopped++;

                    group.grid.style.userSelect = "auto";

                    //reinstate fractional dimensions
                    if (group.orientation === "horizontal")
                    {
                        let newTemplateCols = calculateFracFromPixels(
                            group.grid.offsetWidth,
                            group.dragInfo.currentX - group.dragInfo.initialX,
                            group.columnsDefault.split(" "),
                            //group.grid.style.gridTemplateColumns.split(" "),
                            group.columnIndices);

                        group.columnsDefault = newTemplateCols;
                        group.grid.style.gridTemplateColumns = newTemplateCols;
                            
                    }
                    else if (group.orientation === "vertical")
                    {
                        //let newTemplateRows = calculateFracFromPixels(
                        //    group.rowsDefault.split(" "),
                        //    group.grid.style.gridTemplateRows.split(" "),
                        //    group.rowIndices);
                        let newTemplateRows = calculateFracFromPixels(
                            group.grid.offsetHeight,
                            group.dragInfo.currentY - group.dragInfo.initialY,
                            group.rowsDefault.split(" "),
                            //group.grid.style.gridTemplateRows.split(" "),
                            group.rowIndices);

                        group.rowsDefault = newTemplateRows;
                        group.grid.style.gridTemplateRows = newTemplateRows;
                    }

                }
                group.dragInfo = {
                    isDragging: false,
                    currentX: 0,
                    currentY: 0,
                    initialX: 0,
                    initialY: 0,
                    initialPaneColSizes: [],
                    initialPaneRowSizes: [],
                };
            }
            
            //if (draggersStopped > 0)
            //    console.log("drag ended");
        };

        let findGroupBySplitterControl = function (splitter) {
            for (let i = 0; i < _gridsplittergroups.length; i++) {
                if (_gridsplittergroups[i].control === splitter) {
                    return _gridsplittergroups[i];
                }
            }
            return null;
        };

        let dragStart = function (element, event) {
            let group = findGroupBySplitterControl(element);
            if (group == null) return;

            let gridCompStyles = window.getComputedStyle(group.grid);
            let paneColSizes = gridCompStyles.gridTemplateColumns.split(' ');
            let paneRowSizes = gridCompStyles.gridTemplateRows.split(' ');

            for (let i = 0; i < paneColSizes.length; i++)
                paneColSizes[i] = parseFloat(paneColSizes[i].replace('px', ''));
            for (let i = 0; i < paneRowSizes.length; i++)
                paneRowSizes[i] = parseFloat(paneRowSizes[i].replace('px', ''));

            let initialX = 0;
            let initialY = 0;
            if (event.type === "touchstart") {
                initialX = event.touches[0].clientX;
                initialY = event.touches[0].clientY;
            } else {
                initialX = event.clientX;
                initialY = event.clientY;
            }

            group.dragInfo = {
                isDragging: true,
                currentX: 0, //current cursor X
                currentY: 0, //current cursor Y
                initialX: initialX, //initial cursor X
                initialY: initialY, //initial cursor Y
                initialPaneColSizes: paneColSizes,
                initialPaneRowSizes: paneRowSizes,
            };

            document.addEventListener('mousemove', dragging, false);
            document.addEventListener('mouseup', dragStop, false);
            document.addEventListener('touchmove', dragging, false);
            document.addEventListener('touchend', dragStop, false);

            //console.log("drag started", group.dragInfo);
        };

        

        const _gridsplitters = document.getElementsByClassName("gridsplitter-control");
        const _gridsplittergroups = [];

        for (let i = 0; i < _gridsplitters.length; i++) {
            let control = _gridsplitters[i];
            let gridid = control.getAttribute("data-splitter-grid-id");
            let grid = document.querySelector('#' + gridid);
            let gridColumnsDefault = grid.style.gridTemplateColumns;
            let gridRowsDefault = grid.style.gridTemplateRows;

            let orientation = control.getAttribute("data-splitter-orientation");
            let columnIndices = [];
            let rowIndices = [];
            if (orientation === "horizontal")
                columnIndices = control.getAttribute("data-splitter-columns").split(' ');
            else if (orientation === "vertical")
                rowIndices = control.getAttribute("data-splitter-rows").split(' ');
            
            //let gridCompStyles = window.getComputedStyle(grid);
            //gridColumnsDefault = gridCompStyles.gridTemplateColumns;
            //gridRowsDefault = gridCompStyles.gridTemplateRows;

            let gsgroup = {
                control: control,
                grid: grid,
                orientation: control.getAttribute("data-splitter-orientation"),
                columnsDefault: gridColumnsDefault,
                rowsDefault: gridRowsDefault,
                columnIndices: columnIndices,
                rowIndices: rowIndices,
                dragInfo: {
                    isDragging: false,
                    currentX: 0, //current cursor X
                    currentY: 0, //current cursor Y
                    initialX: 0, //initial cursor X
                    initialY: 0, //initial cursor Y
                    initialPaneColSizes: [],
                    initialPaneRowSizes: [],
                } //holds dragging info during dragging
            };

            gsgroup.control.addEventListener('mousedown', function (event) {
                dragStart(this, event);
            }, false);
            gsgroup.control.addEventListener('touchstart', function (event) {
                dragStart(this, event);
            }, false);

            _gridsplittergroups.push(gsgroup);
        }
    }
}()).init();
