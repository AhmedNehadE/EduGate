// Add this to your site.js file

$(document).ready(function () {
    // Set up search for both desktop and mobile
    setupSearch('#courseSearchInput', '#courseSearchForm', '#searchResults');
    setupSearch('#mobileSearchInput', '#mobileSearchForm', '#mobileSearchResults');

    function setupSearch(inputSelector, formSelector, resultsSelector) {
        const searchInput = $(inputSelector);
        const searchForm = $(formSelector);
        const searchResults = $(resultsSelector);

        // Minimum characters to trigger search
        const minSearchLength = 2;

        // Current AJAX request
        let currentRequest = null;

        // Debounce function to limit API calls
        function debounce(func, wait) {
            let timeout;
            return function (...args) {
                const context = this;
                clearTimeout(timeout);
                timeout = setTimeout(() => func.apply(context, args), wait);
            };
        }

        // Search input handler with debounce
        searchInput.on('input', debounce(function () {
            const query = $(this).val().trim();

            // Clear results if query is too short
            if (query.length < minSearchLength) {
                searchResults.empty().hide();
                return;
            }

            // Cancel previous request if exists
            if (currentRequest) {
                currentRequest.abort();
            }

            // Make AJAX call to search API
            currentRequest = $.ajax({
                url: '/Course/SearchSuggestions',
                method: 'GET',
                data: { query: query },
                success: function (data) {
                    // Clear previous results
                    searchResults.empty();

                    if (data && data.length > 0) {
                        // Create dropdown items for each course
                        data.forEach(course => {
                            searchResults.append(`
                                <a class="dropdown-item d-flex align-items-center py-2" href="/Course/Details/${course.id}">
                                    <div class="me-2">
                                        <img src="${course.thumbnailUrl}" 
                                             alt="${course.title}" 
                                             width="40" height="40" 
                                             class="rounded" 
                                             onerror="this.onerror=null; this.src='/img/default-course.png';">
                                    </div>
                                    <div>
                                        <div class="fw-bold">${course.title}</div>
                                        <small class="text-muted">${course.instructorName} • ${course.category}</small>
                                    </div>
                                </a>
                            `);
                        });

                        // Add a "See all results" option
                        searchResults.append(`
                            <div class="dropdown-divider"></div>
                            <a class="dropdown-item text-primary text-center" href="/Course/Search?query=${encodeURIComponent(query)}">
                                See all results for "${query}"
                            </a>
                        `);

                        // Show the dropdown
                        searchResults.show();
                    } else {
                        searchResults.html(`
                            <span class="dropdown-item text-muted">No courses found</span>
                        `).show();
                    }
                },
                error: function (xhr, status, error) {
                    if (status !== 'abort') {
                        console.error('Error in search:', error);
                    }
                }
            });
        }, 300));

        // Handle form submission
        searchForm.on('submit', function (e) {
            e.preventDefault();
            const query = searchInput.val().trim();
            if (query.length >= minSearchLength) {
                window.location.href = `/Course/Search?query=${encodeURIComponent(query)}`;
            }
        });

        // Handle keyboard navigation in dropdown
        searchInput.on('keydown', function (e) {
            const items = searchResults.find('.dropdown-item');
            const current = searchResults.find('.dropdown-item.active');

            switch (e.keyCode) {
                case 40: // Down arrow
                    e.preventDefault();
                    if (current.length) {
                        current.removeClass('active');
                        const next = current.next('.dropdown-item');
                        if (next.length) next.addClass('active');
                        else items.first().addClass('active');
                    } else {
                        items.first().addClass('active');
                    }
                    break;

                case 38: // Up arrow
                    e.preventDefault();
                    if (current.length) {
                        current.removeClass('active');
                        const prev = current.prev('.dropdown-item');
                        if (prev.length) prev.addClass('active');
                        else items.last().addClass('active');
                    } else {
                        items.last().addClass('active');
                    }
                    break;

                case 13: // Enter
                    if (current.length && current.attr('href')) {
                        e.preventDefault();
                        window.location.href = current.attr('href');
                    }
                    break;

                case 27: // Escape
                    e.preventDefault();
                    searchResults.hide();
                    break;
            }
        });
    }

    // Hide dropdown when clicking outside
    $(document).on('click', function (e) {
        if (!$(e.target).closest('#searchContainer, #mobileSearchContainer').length) {
            $('#searchResults, #mobileSearchResults').hide();
        }
    });
});