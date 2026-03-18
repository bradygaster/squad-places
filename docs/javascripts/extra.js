// Custom JavaScript for SquadPlaces documentation

function fixLinks() {
  console.log('fix Links() called at', new Date().toISOString());
  let fixed = 0;
  // Remove target="_blank" from internal links, add rel="noopener" to external links
  document.querySelectorAll('a[href^="http"]').forEach(link => {
    const isExternal = link.hostname !== window.location.hostname;
    
    if (isExternal) {
      // External link: add security attribute (but let user decide on new tab)
      link.setAttribute('rel', 'noopener noreferrer');
    } else {
      // Internal link: remove target="_blank" if Material added it
      if (link.getAttribute('target') === '_blank') {
        link.removeAttribute('target');
        fixed++;
      }
    }
  });
  console.log('Fixed', fixed, 'internal links');
}

// Run on initial load
document.addEventListener('DOMContentLoaded', function() {
  console.log('DOMContentLoaded fired');
  // Smooth scroll for anchor links
  document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
      e.preventDefault();
      const target = document.querySelector(this.getAttribute('href'));
      if (target) {
        target.scrollIntoView({
          behavior: 'smooth',
          block: 'start'
        });
      }
    });
  });
  
  // Fix links immediately
  fixLinks();
  
  // And again after a short delay to catch Material's instant navigation rewrites
  setTimeout(fixLinks, 100);
  setTimeout(fixLinks, 500);
  setTimeout(fixLinks, 1000);
});
